using System;
using System.Collections.Generic;
using UnityEngine;

namespace CupkekGames.Quests
{
    /// <summary>
    /// Static authored quest template — held by <see cref="QuestSO"/>, cloned into a runtime <see cref="QuestInstance"/>
    /// when the player accepts the quest. All lists are <see cref="SerializeReference"/>-polymorphic so designers
    /// author objectives / actions / conditions / rewards / features inline (no wrapper-SO clutter).
    /// </summary>
    [Serializable]
    public class QuestDefinition
    {
        public string Name;
        [TextArea(3, 6)] public string Description;

        /// <summary>Objective templates. Cloned per runtime instance; runtime progress lives on the cloned objectives.</summary>
        [SerializeReference] public List<QuestObjective> Objectives = new List<QuestObjective>();

        /// <summary>Actions executed when the quest first starts.</summary>
        [SerializeReference] public List<QuestAction> OnStartActions = new List<QuestAction>();

        /// <summary>Actions executed when all objectives are done and the quest is ready to turn in.</summary>
        [SerializeReference] public List<QuestAction> OnReadyActions = new List<QuestAction>();

        /// <summary>Actions executed when the quest is completed.</summary>
        [SerializeReference] public List<QuestAction> OnCompleteActions = new List<QuestAction>();

        /// <summary>Conditions gating start actions.</summary>
        [SerializeReference] public List<QuestActionCondition> OnStartActionsConditions = new List<QuestActionCondition>();

        /// <summary>Conditions gating ready actions.</summary>
        [SerializeReference] public List<QuestActionCondition> OnReadyActionsConditions = new List<QuestActionCondition>();

        /// <summary>Conditions gating complete actions.</summary>
        [SerializeReference] public List<QuestActionCondition> OnCompleteActionsConditions = new List<QuestActionCondition>();

        /// <summary>Composable rewards — multiple per quest (gold + items + XP + reputation, etc.).</summary>
        [SerializeReference] public List<IQuestRewardFeature> Rewards = new List<IQuestRewardFeature>();

        /// <summary>Game-specific extension data (e.g. NPC client reference, quest type, region).</summary>
        [SerializeReference] public List<IQuestFeature> Features = new List<IQuestFeature>();
    }
}
