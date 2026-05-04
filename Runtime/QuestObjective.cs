using System;
using System.Collections.Generic;
using UnityEngine;

namespace CupkekGames.Quests
{
    /// <summary>
    /// Abstract base for quest objectives.
    /// Authored fields (RequiredProgress, BlockNextObjective, action lists) are inspector-set on each
    /// <c>[SerializeReference]</c> entry inside <see cref="QuestDefinition.Objectives"/>.
    /// Runtime fields (Progress, QuestId, _quest) are set by the lifecycle.
    /// Concrete subclasses must provide a parameterless constructor for SerializeReference deserialization.
    /// </summary>
    [Serializable]
    public abstract class QuestObjective
    {
        /// <summary>Set at runtime by <see cref="QuestSO.CreateInstance"/> — empty in templates.</summary>
        public Guid QuestId;

        /// <summary>Authored: amount of progress needed to complete this objective.</summary>
        public int RequiredProgress;

        /// <summary>Runtime: current progress toward <see cref="RequiredProgress"/>.</summary>
        public int Progress;

        /// <summary>Authored: when true, subsequent objectives in the quest don't activate until this one completes.</summary>
        public bool BlockNextObjective;

        [SerializeReference] public List<QuestAction> OnStartActions = new List<QuestAction>();
        [SerializeReference] public List<QuestAction> OnCompleteActions = new List<QuestAction>();
        [SerializeReference] public List<QuestActionCondition> OnStartActionsConditions = new List<QuestActionCondition>();
        [SerializeReference] public List<QuestActionCondition> OnCompleteActionsConditions = new List<QuestActionCondition>();

        [NonSerialized] protected QuestInstance _quest;

        /// <summary>Required for SerializeReference deserialization.</summary>
        protected QuestObjective() { }

        public virtual bool IsCompleted() => Progress >= RequiredProgress;

        public abstract string GetDescription();

        public abstract void Activate();

        public abstract void Deactivate();

        public virtual void OnStart(QuestInstance quest)
        {
            _quest = quest;

            foreach (QuestActionCondition condition in OnStartActionsConditions)
            {
                if (!condition.CanExecute()) return;
            }

            foreach (QuestAction action in OnStartActions)
            {
                action.Execute(quest);
            }
        }

        protected void AddProgress(int amount = 1)
        {
            if (IsCompleted()) return;
            Progress += amount;
            if (IsCompleted()) OnComplete();
        }

        protected virtual void OnComplete()
        {
            foreach (QuestActionCondition condition in OnCompleteActionsConditions)
            {
                if (!condition.CanExecute()) return;
            }

            foreach (QuestAction action in OnCompleteActions)
            {
                action.Execute(_quest);
            }
        }

        /// <summary>
        /// Deep clone for instantiation — caller assigns <see cref="QuestId"/> and resets <see cref="Progress"/>.
        /// Default impl uses <see cref="UnityEngine.JsonUtility"/> for SerializeReference-friendly cloning;
        /// override if your subclass holds non-serialized state that needs special handling.
        /// </summary>
        public virtual QuestObjective Clone()
        {
            string json = JsonUtility.ToJson(this);
            QuestObjective clone = (QuestObjective)JsonUtility.FromJson(json, GetType());
            clone.Progress = 0;
            clone.QuestId = Guid.Empty;
            return clone;
        }
    }
}
