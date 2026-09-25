using System;
using System.Collections.Generic;
using UnityEngine;

namespace CupkekGames.Quests
{
    /// <summary>
    /// What a quest is: its text, objectives, rewards and game data. Authored inside a
    /// <see cref="QuestSO"/>, or built at runtime by a generator and saved inline on its
    /// <see cref="QuestState"/>. Never mutated once a quest is in a log: progress lives on
    /// the state.
    /// </summary>
    [Serializable]
    public class QuestDefinition
    {
        public string Title;
        [TextArea(3, 6)] public string Description;

        /// <summary>Objectives complete in order: only the first open one takes progress.</summary>
        public bool Ordered;

        public List<QuestObjectiveDefinition> Objectives = new List<QuestObjectiveDefinition>();

        /// <summary>Granted by <see cref="QuestLog.TurnIn"/>, in order.</summary>
        [SerializeReference] public List<IQuestReward> Rewards = new List<IQuestReward>();

        /// <summary>Game data the log never reads: the giver, the source, a story line.</summary>
        [SerializeReference] public List<IQuestFeature> Features = new List<IQuestFeature>();

        /// <summary>The first feature of type <typeparamref name="T"/>, or null.</summary>
        public T GetFeature<T>() where T : class, IQuestFeature
        {
            foreach (IQuestFeature feature in Features)
            {
                if (feature is T typed) return typed;
            }
            return null;
        }
    }
}
