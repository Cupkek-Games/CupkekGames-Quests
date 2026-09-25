using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace CupkekGames.Quests
{
    /// <summary>
    /// A quest in a log: the only part that is saved. An authored quest saves its catalog
    /// <see cref="Key"/>; a generated one saves its definition inline (<see cref="Inline"/>).
    /// The definition itself is resolved by the log (<see cref="QuestLog.Bind"/>).
    /// Opt-in data contract: only the members marked <see cref="DataMemberAttribute"/>
    /// are saved; the resolved definition and the computed state are not.
    /// </summary>
    [Serializable, DataContract]
    public class QuestState
    {
        [DataMember] public Guid Id { get; private set; }

        /// <summary>The authored quest's catalog key; empty for a generated quest.</summary>
        [DataMember] public string Key { get; private set; }

        /// <summary>A generated quest's definition; null for an authored one.</summary>
        [DataMember] public QuestDefinition Inline { get; private set; }

        /// <summary>Progress per objective, in the definition's order.</summary>
        [DataMember] public List<int> Progress { get; private set; } = new List<int>();

        [DataMember] public QuestStatus Status { get; internal set; }

        /// <summary>Time left before the quest fails, in the game's units; -1: no deadline.</summary>
        [DataMember] public int Deadline { get; internal set; } = -1;

        /// <summary>Resolved by the log; never saved.</summary>
        [NonSerialized] private QuestDefinition _definition;
        public QuestDefinition Definition => _definition;

        /// <summary>For the serializer.</summary>
        public QuestState() { }

        internal QuestState(string key, QuestDefinition inline, int deadline)
        {
            Id = Guid.NewGuid();
            Key = key;
            Inline = inline;
            Deadline = deadline > 0 ? deadline : -1;
        }

        internal QuestState(QuestState other)
        {
            Id = other.Id;
            Key = other.Key;
            Inline = other.Inline;
            Progress = new List<int>(other.Progress);
            Status = other.Status;
            Deadline = other.Deadline;
            _definition = other._definition;
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
                Progress[i] = Math.Min(Progress[i], Math.Max(0, definition.Objectives[i].Required));
            }
        }

        public bool IsOpen => Status == QuestStatus.Active || Status == QuestStatus.Ready;

        public bool IsObjectiveDone(int index)
            => Progress[index] >= _definition.Objectives[index].Required;

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
