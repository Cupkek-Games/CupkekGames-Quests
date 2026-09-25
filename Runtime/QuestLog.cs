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

        [NonSerialized] private Func<string, QuestDefinition> _resolve;

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
            foreach (QuestState quest in other.Quests) Quests.Add(new QuestState(quest));
            _resolve = other._resolve;
        }

        /// <summary>
        /// Sets how authored keys resolve and attaches every quest's definition. A saved key
        /// that no longer resolves is an error: the game removed a quest a save still holds.
        /// </summary>
        public void Bind(Func<string, QuestDefinition> resolve)
        {
            _resolve = resolve ?? throw new ArgumentNullException(nameof(resolve));
            foreach (QuestState quest in Quests) quest.Attach(ResolveFor(quest));
        }

        /// <summary>Binds against a catalog.</summary>
        public void Bind(QuestCatalog catalog)
        {
            if (catalog == null) throw new ArgumentNullException(nameof(catalog));
            Bind(key => catalog.GetValue(key)?.Definition);
        }

        // ── Adding ──────────────────────────────────────────────

        /// <summary>Adds the authored quest <paramref name="key"/>.</summary>
        public QuestState Add(string key, int deadline = -1)
        {
            if (string.IsNullOrEmpty(key)) throw new ArgumentException("An authored quest needs a key.", nameof(key));
            return AddState(new QuestState(key, null, deadline));
        }

        /// <summary>Adds a generated quest; its definition is saved with it.</summary>
        public QuestState Add(QuestDefinition definition, int deadline = -1)
        {
            if (definition == null) throw new ArgumentNullException(nameof(definition));
            return AddState(new QuestState(null, definition, deadline));
        }

        private QuestState AddState(QuestState quest)
        {
            quest.Attach(ResolveFor(quest));
            Quests.Add(quest);
            Added?.Invoke(quest);
            // A quest with nothing left to do is ready at once (unless an Added
            // handler's report already moved it on).
            if (quest.Status == QuestStatus.Active && quest.AllObjectivesDone()) MarkReady(quest);
            return quest;
        }

        private QuestDefinition ResolveFor(QuestState quest)
        {
            if (quest.Inline != null) return quest.Inline;
            if (_resolve == null) throw new InvalidOperationException("QuestLog.Bind was not called.");
            QuestDefinition definition = _resolve(quest.Key);
            if (definition == null) throw new InvalidOperationException($"No quest definition for key '{quest.Key}'.");
            return definition;
        }

        // ── Progress ────────────────────────────────────────────

        /// <summary>Something happened <paramref name="amount"/> times: adds to every live objective it matches.</summary>
        public void Signal(string kind, string key = null, int amount = 1)
        {
            if (amount <= 0) return;
            Apply(kind, key, (current, required) => current + amount);
        }

        /// <summary>
        /// A value the game owns now stands at <paramref name="value"/> (a level, a count
        /// of cleared regions): matching live objectives rise to it, never fall.
        /// </summary>
        public void Report(string kind, string key, int value)
        {
            Apply(kind, key, (current, required) => Math.Max(current, value));
        }

        private void Apply(string kind, string key, Func<int, int, int> next)
        {
            // Snapshot: a handler may add or remove quests.
            foreach (QuestState quest in Quests.ToArray())
            {
                if (quest.Status != QuestStatus.Active) continue;
                List<QuestObjectiveDefinition> objectives = quest.Definition.Objectives;
                bool moved = false;
                for (int i = 0; i < objectives.Count; i++)
                {
                    if (!quest.IsObjectiveLive(i) || !objectives[i].Matches(kind, key)) continue;
                    int required = objectives[i].Required;
                    int value = Math.Min(required, next(quest.Progress[i], required));
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
            foreach (IQuestReward reward in quest.Definition.Rewards) reward.Grant();
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

        /// <summary>The latest quest added from the authored <paramref name="key"/>, or null.</summary>
        public QuestState FindByKey(string key) => Quests.FindLast(q => q.Key == key);

        public bool HasCompleted(string key) => Quests.Exists(q => q.Key == key && q.Status == QuestStatus.Completed);

        // ── IData ───────────────────────────────────────────────

        public bool Validate() => Quests != null;

        public void OnAfterDeserialize() { }

        /// <summary>A deep copy of the states; definitions are shared (they never change in a log).</summary>
        public IData CloneData() => new QuestLog(this);
    }
}
