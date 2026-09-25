using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using CupkekGames.Data;

namespace CupkekGames.Quests
{
    /// <summary>
    /// A quest a player has: the only part that is saved. It names its definition by
    /// <see cref="Key"/> (an authored quest, or the template a generated quest rolled from)
    /// and holds the runtime side: progress, status, deadline and per-feature state
    /// (<see cref="IFeatureStateData"/>, e.g. a <see cref="QuestRoll"/>). A definition is
    /// never saved; the log attaches it (<see cref="QuestLog.Attach"/>).
    /// Opt-in data contract: only the members marked <see cref="DataMemberAttribute"/> are saved.
    /// </summary>
    [Serializable, DataContract]
    public class QuestState
    {
        [DataMember] public Guid Id { get; private set; }

        /// <summary>The definition's catalog key.</summary>
        [DataMember] public CatalogKey Key { get; private set; }

        /// <summary>Progress per objective, in the definition's order.</summary>
        [DataMember] public List<int> Progress { get; private set; } = new List<int>();

        [DataMember] public QuestStatus Status { get; internal set; }

        /// <summary>Time left before the quest fails, in the game's units; -1: no deadline.</summary>
        [DataMember] public int Deadline { get; internal set; } = -1;

        [DataMember] private List<IFeatureStateData> _featureState = new List<IFeatureStateData>();

        /// <summary>Attached by the log; never saved.</summary>
        [NonSerialized] private QuestDefinition _definition;
        public QuestDefinition Definition => _definition;

        /// <summary>For the serializer.</summary>
        public QuestState() { }

        /// <summary>A new quest from the definition at <paramref name="key"/>; add it to a log, or attach it to show it.</summary>
        public QuestState(CatalogKey key, int deadline = -1)
        {
            Id = Guid.NewGuid();
            Key = key;
            Deadline = deadline > 0 ? deadline : -1;
        }

        /// <summary>A deep copy: same id, progress and feature state; the definition is shared.</summary>
        public QuestState Clone() => new QuestState(this);

        private QuestState(QuestState other)
        {
            Id = other.Id;
            Key = other.Key;
            Progress = new List<int>(other.Progress);
            Status = other.Status;
            Deadline = other.Deadline;
            foreach (IFeatureStateData state in other._featureState) _featureState.Add(state?.CloneState());
            _definition = other._definition;
        }

        // ── Feature state ───────────────────────────────────────

        public T GetState<T>() where T : class, IFeatureStateData
        {
            foreach (IFeatureStateData state in _featureState)
            {
                if (state is T typed) return typed;
            }
            return null;
        }

        public T GetOrCreateState<T>() where T : class, IFeatureStateData, new()
        {
            T existing = GetState<T>();
            if (existing != null) return existing;
            T created = new T();
            _featureState.Add(created);
            return created;
        }

        // ── Objectives (the definition's, as this quest rolled them) ──

        public int ObjectiveCount => _definition.Objectives.Count;

        public string ObjectiveKind(int index) => _definition.Objectives[index].Kind;

        public string ObjectiveKey(int index)
        {
            string rolled = GetState<QuestRoll>()?.KeyAt(index);
            return string.IsNullOrEmpty(rolled) ? _definition.Objectives[index].Key : rolled;
        }

        public int ObjectiveRequired(int index)
        {
            int rolled = GetState<QuestRoll>()?.RequiredAt(index) ?? 0;
            return rolled > 0 ? rolled : _definition.Objectives[index].Required;
        }

        public bool ObjectiveMatches(int index, string kind, string key)
        {
            if (ObjectiveKind(index) != kind) return false;
            string wanted = ObjectiveKey(index);
            return string.IsNullOrEmpty(wanted) || wanted == key;
        }

        /// <summary>
        /// Attaches the definition and fits the progress list to its objectives (a quest
        /// edited since the save keeps what still lines up).
        /// </summary>
        internal void Attach(QuestDefinition definition)
        {
            _definition = definition;
            int count = definition.Objectives.Count;
            while (Progress.Count < count) Progress.Add(0);
            if (Progress.Count > count) Progress.RemoveRange(count, Progress.Count - count);
            for (int i = 0; i < count; i++)
            {
                Progress[i] = Math.Min(Progress[i], Math.Max(0, ObjectiveRequired(i)));
            }
        }

        public bool IsOpen => Status == QuestStatus.Active || Status == QuestStatus.Ready;

        public bool IsObjectiveDone(int index) => Progress[index] >= ObjectiveRequired(index);

        public bool AllObjectivesDone()
        {
            for (int i = 0; i < Progress.Count; i++)
            {
                if (!IsObjectiveDone(i)) return false;
            }
            return true;
        }

        /// <summary>The first objective not yet done, or -1.</summary>
        public int FirstOpenObjective()
        {
            for (int i = 0; i < Progress.Count; i++)
            {
                if (!IsObjectiveDone(i)) return i;
            }
            return -1;
        }

        /// <summary>Whether objective <paramref name="index"/> takes progress now (an ordered quest: only the first open one).</summary>
        public bool IsObjectiveLive(int index)
        {
            if (Status != QuestStatus.Active || IsObjectiveDone(index)) return false;
            return !_definition.Ordered || FirstOpenObjective() == index;
        }
    }
}
