using System;

namespace CupkekGames.Quests
{
    /// <summary>
    /// One objective, as plain data: progress comes from signals whose kind matches
    /// <see cref="Kind"/> and whose key matches <see cref="Key"/> (an empty key accepts any).
    /// The game owns the vocabulary of kinds and keys and posts the signals
    /// (<see cref="QuestLog.Signal"/>, <see cref="QuestLog.Report"/>). A generated quest may
    /// replace the key and the count per quest (<see cref="QuestRoll"/>).
    /// </summary>
    [Serializable]
    public class QuestObjectiveDefinition
    {
        /// <summary>What happens, e.g. "cook", "hire", "boss". Defined by the game.</summary>
        public string Kind;

        /// <summary>What it happens to, e.g. a meal key. Empty: any key counts.</summary>
        public string Key;

        /// <summary>How much progress completes the objective.</summary>
        public int Required = 1;

        /// <summary>Optional authored text; empty lets the game describe the objective from its kind.</summary>
        public string Text;

        public QuestObjectiveDefinition() { }

        public QuestObjectiveDefinition(string kind, string key = null, int required = 1)
        {
            Kind = kind;
            Key = key;
            Required = required;
        }
    }
}
