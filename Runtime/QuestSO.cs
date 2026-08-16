using System;
using UnityEngine;

namespace CupkekGames.Quests
{
    /// <summary>
    /// Bare ScriptableObject holding a static authored <see cref="QuestDefinition"/> template.
    /// <see cref="CreateInstance"/> clones the definition into a runtime <see cref="QuestInstance"/>.
    /// </summary>
    [CreateAssetMenu(fileName = "Quest", menuName = "CupkekGames/Quests/Quest")]
    public class QuestSO : ScriptableObject
    {
        public QuestDefinition Definition = new QuestDefinition();

        /// <summary>
        /// Clone the definition into a fresh runtime instance with a new ID.
        /// Each objective, action, condition, reward, and feature is deep-cloned.
        /// </summary>
        public virtual QuestInstance CreateInstance()
        {
            QuestInstance instance = new QuestInstance();
            instance.DefinitionKey = name;
            instance.Name = Definition.Name;
            instance.Description = Definition.Description;
            instance.TimeLeft = -1;

            foreach (QuestObjective template in Definition.Objectives)
            {
                if (template == null) continue;
                QuestObjective clone = template.Clone();
                clone.QuestId = instance.Id;
                instance.Objectives.Add(clone);
            }

            CopyActions(Definition.OnStartActions, instance.OnStartActions);
            CopyActions(Definition.OnReadyActions, instance.OnReadyActions);
            CopyActions(Definition.OnCompleteActions, instance.OnCompleteActions);
            CopyConditions(Definition.OnStartActionsConditions, instance.OnStartActionsConditions);
            CopyConditions(Definition.OnReadyActionsConditions, instance.OnReadyActionsConditions);
            CopyConditions(Definition.OnCompleteActionsConditions, instance.OnCompleteActionsConditions);

            foreach (IQuestRewardFeature reward in Definition.Rewards)
            {
                if (reward != null) instance.Rewards.Add(reward.CloneFeature());
            }

            foreach (IQuestFeature feature in Definition.Features)
            {
                if (feature != null) instance.Features.Add(feature.CloneFeature());
            }

            return instance;
        }

        private static void CopyActions(System.Collections.Generic.List<QuestAction> from, System.Collections.Generic.List<QuestAction> to)
        {
            foreach (QuestAction a in from)
            {
                if (a != null) to.Add(a);
            }
        }

        private static void CopyConditions(System.Collections.Generic.List<QuestActionCondition> from, System.Collections.Generic.List<QuestActionCondition> to)
        {
            foreach (QuestActionCondition c in from)
            {
                if (c != null) to.Add(c);
            }
        }
    }
}
