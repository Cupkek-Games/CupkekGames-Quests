using System;
using System.Collections.Generic;
using CupkekGames.Data;

namespace CupkekGames.Quests
{
    /// <summary>
    /// The quests a player has, and the one place their progress changes. The game posts
    /// what happened (<see cref="Signal"/>, <see cref="Report"/>); the log moves matching
    /// objectives, marks quests Ready, and completes them on <see cref="TurnIn"/>.
    /// Saved as part of the game state; call <see cref="Bind"/> after creating or loading
    /// one, before anything else.
    /// </summary>
    [Serializable]
    public class QuestLog : IData
    {
        public List<QuestState> Quests { get; private set; } = new List<QuestState>();

        [NonSerialized] private Func<CatalogKey, QuestDefinition> _resolve;

        public event Action<QuestState> Added;
        /// <summary>An objective moved (the quest, the objective's index).</summary>
        public event Action<QuestState, int> Progressed;
        public event Action<QuestState> Ready;
        public event Action<QuestState> Completed;
        public event Action<QuestState> Failed;
        public event Action<QuestState> Removed;

        public QuestLog() { }

        private QuestLog(QuestLog other)
        {
            foreach (QuestState quest in other.Quests) Quests.Add(quest.Clone());
            _resolve = other._resolve;
        }

        /// <summary>
        /// Sets how keys resolve to definitions (usually the game's quest catalog) and
        /// attaches every quest. A saved key that no longer resolves is an error: the game
        /// removed a quest a save still holds.
        /// </summary>
        public void Bind(Func<CatalogKey, QuestDefinition> resolve)
        {
            _resolve = resolve ?? throw new ArgumentNullException(nameof(resolve));
            foreach (QuestState quest in Quests) Attach(quest);
        }

        /// <summary>Attaches a quest's definition without adding it (a quest shown before it is taken, like an offer on a board).</summary>
        public void Attach(QuestState quest)
        {
            if (_resolve == null) throw new InvalidOperationException("QuestLog.Bind was not called.");
            QuestDefinition definition = _resolve(quest.Key);
            if (definition == null) throw new InvalidOperationException($"No quest definition for key '{quest.Key.Catalog}/{quest.Key.Key}'.");
            quest.Attach(definition);
        }

        // ── Adding ──────────────────────────────────────────────

        /// <summary>Adds the quest at <paramref name="key"/>.</summary>
        public QuestState Add(CatalogKey key, int deadline = -1) => Add(new QuestState(key, deadline));

        /// <summary>Adds a prepared quest (a generated one carries its roll as feature state).</summary>
        public QuestState Add(QuestState quest)
        {
            if (quest == null) throw new ArgumentNullException(nameof(quest));
            if (Quests.Contains(quest)) throw new InvalidOperationException("The quest is already in the log.");
            Attach(quest);
            Quests.Add(quest);
            Added?.Invoke(quest);
            // A quest with nothing left to do is ready at once (unless an Added
            // handler's report already moved it on).
            if (quest.Status == QuestStatus.Active && quest.AllObjectivesDone()) MarkReady(quest);
            return quest;
        }

        // ── Progress ────────────────────────────────────────────

        /// <summary>Something happened <paramref name="amount"/> times: adds to every live objective it matches.</summary>
        public void Signal(string kind, string key = null, int amount = 1)
        {
            if (amount <= 0) return;
            Apply(kind, key, current => current + amount);
        }

        /// <summary>
        /// A value the game owns now stands at <paramref name="value"/> (a level, a count
        /// of cleared regions): matching live objectives rise to it, never fall.
        /// </summary>
        public void Report(string kind, string key, int value)
        {
            Apply(kind, key, current => Math.Max(current, value));
        }

        private void Apply(string kind, string key, Func<int, int> next)
        {
            // Snapshot: a handler may add or remove quests.
            foreach (QuestState quest in Quests.ToArray())
            {
                if (quest.Status != QuestStatus.Active) continue;
                bool moved = false;
                for (int i = 0; i < quest.ObjectiveCount; i++)
                {
                    if (!quest.IsObjectiveLive(i) || !quest.ObjectiveMatches(i, kind, key)) continue;
                    int value = Math.Min(quest.ObjectiveRequired(i), next(quest.Progress[i]));
                    if (value == quest.Progress[i]) continue;
                    quest.Progress[i] = value;
                    moved = true;
                    Progressed?.Invoke(quest, i);
                    // An ordered quest takes one objective per signal.
                    if (quest.Definition.Ordered) break;
                }
                if (moved && quest.AllObjectivesDone()) MarkReady(quest);
            }
        }

        private void MarkReady(QuestState quest)
        {
            quest.Status = QuestStatus.Ready;
            Ready?.Invoke(quest);
        }

        // ── Ending ──────────────────────────────────────────────

        /// <summary>Completes a Ready quest and grants its rewards.</summary>
        public void TurnIn(QuestState quest)
        {
            if (quest == null) throw new ArgumentNullException(nameof(quest));
            if (quest.Status != QuestStatus.Ready)
                throw new InvalidOperationException($"Quest '{quest.Definition.Title}' is {quest.Status}, not Ready.");
            quest.Status = QuestStatus.Completed;
            foreach (IQuestReward reward in quest.Definition.Rewards) reward.Grant(quest);
            Completed?.Invoke(quest);
        }

        /// <summary>Fails an open quest.</summary>
        public void Fail(QuestState quest)
        {
            if (quest == null || !quest.IsOpen) return;
            quest.Status = QuestStatus.Failed;
            Failed?.Invoke(quest);
        }

        public void Remove(QuestState quest)
        {
            if (quest == null || !Quests.Remove(quest)) return;
            Removed?.Invoke(quest);
        }

        /// <summary>Time passed: open quests with a deadline lose <paramref name="amount"/>; one that reaches 0 fails.</summary>
        public void Tick(int amount)
        {
            if (amount <= 0) return;
            foreach (QuestState quest in Quests.ToArray())
            {
                if (!quest.IsOpen || quest.Deadline < 0) continue;
                quest.Deadline = Math.Max(0, quest.Deadline - amount);
                if (quest.Deadline == 0) Fail(quest);
            }
        }

        // ── Queries ─────────────────────────────────────────────

        public QuestState Find(Guid id) => Quests.Find(q => q.Id == id);

        /// <summary>The latest quest added from <paramref name="key"/>, or null.</summary>
        public QuestState FindByKey(CatalogKey key) => Quests.FindLast(q => q.Key.Equals(key));

        public bool HasCompleted(CatalogKey key) => Quests.Exists(q => q.Key.Equals(key) && q.Status == QuestStatus.Completed);

        // ── IData ───────────────────────────────────────────────

        public bool Validate() => Quests != null;

        public void OnAfterDeserialize() { }

        /// <summary>A deep copy of the states; definitions are shared (they never change).</summary>
        public IData CloneData() => new QuestLog(this);
    }
}
