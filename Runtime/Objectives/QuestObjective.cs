using System;
using System.Collections.Generic;
using UnityEngine;

namespace CupkekGames.Quests
{
    /// <summary>
    /// Abstract base class for quest objectives.
    /// Each objective tracks progress toward a goal and provides lifecycle hooks.
    /// Game-specific objectives subclass this and implement <see cref="Activate"/>
    /// and <see cref="Deactivate"/> to subscribe/unsubscribe from game events.
    /// Call <see cref="AddProgress"/> from subclasses when the player makes progress.
    /// </summary>
    [Serializable]
    public abstract class QuestObjective
    {
        /// <summary>The ID of the quest this objective belongs to.</summary>
        public Guid QuestId;

        /// <summary>The amount of progress needed to complete this objective.</summary>
        public int RequiredProgress;

        /// <summary>The current progress toward <see cref="RequiredProgress"/>.</summary>
        public int Progress;

        /// <summary>
        /// When true, subsequent objectives in the quest will not activate until this one completes.
        /// </summary>
        public bool BlockNextObjective;

        /// <summary>Actions to run when this objective becomes the active objective.</summary>
        [SerializeReference] public List<QuestAction> OnStartActions = new List<QuestAction>();

        /// <summary>Actions to run when this objective completes.</summary>
        [SerializeReference] public List<QuestAction> OnCompleteActions = new List<QuestAction>();

        /// <summary>Conditions that must all pass before start actions execute.</summary>
        [SerializeReference] public List<QuestActionCondition> OnStartActionsConditions = new List<QuestActionCondition>();

        /// <summary>Conditions that must all pass before complete actions execute.</summary>
        [SerializeReference] public List<QuestActionCondition> OnCompleteActionsConditions = new List<QuestActionCondition>();

        /// <summary>
        /// Fired when this objective completes. Subscribers (e.g. the quest tracker)
        /// re-evaluate quest status and activate the next objective.
        /// </summary>
        [NonSerialized] public Action<QuestObjective> Completed;

        /// <summary>
        /// Reference to the owning quest, set by <see cref="OnStart"/>.
        /// Available to subclasses for context during event handlers.
        /// </summary>
        [NonSerialized] protected Quest _quest;

        public QuestObjective(Guid questId, int requiredProgress, bool blockNextObjective)
        {
            QuestId = questId;
            RequiredProgress = requiredProgress;
            BlockNextObjective = blockNextObjective;
        }

        /// <summary>Returns true when <see cref="Progress"/> has reached <see cref="RequiredProgress"/>.</summary>
        public virtual bool IsCompleted()
        {
            return Progress >= RequiredProgress;
        }

        /// <summary>Get a human-readable description of this objective (e.g. "Kill 3/5 Slimes").</summary>
        public abstract string GetDescription();

        /// <summary>
        /// Subscribe to game events that drive progress.
        /// Called when the quest becomes active or when a prior blocking objective completes.
        /// </summary>
        public abstract void Activate();

        /// <summary>
        /// Unsubscribe from all game events.
        /// Called when the quest is removed, completed, or before re-activation.
        /// </summary>
        public abstract void Deactivate();

        /// <summary>
        /// Called when this objective becomes the current active objective.
        /// Stores the quest reference and runs start actions if all conditions pass.
        /// </summary>
        /// <param name="quest">The quest that owns this objective.</param>
        public virtual void OnStart(Quest quest)
        {
            _quest = quest;

            foreach (QuestActionCondition condition in OnStartActionsConditions)
            {
                if (!condition.CanExecute())
                {
                    return;
                }
            }

            foreach (QuestAction action in OnStartActions)
            {
                action.Execute(quest);
            }
        }

        /// <summary>
        /// Call this from game-specific subclasses when progress is made.
        /// Increments progress and fires completion if the goal is reached.
        /// </summary>
        /// <param name="amount">How much progress to add (default 1).</param>
        protected void AddProgress(int amount = 1)
        {
            if (IsCompleted())
            {
                return;
            }

            Progress += amount;

            if (IsCompleted())
            {
                OnComplete();
            }
        }

        /// <summary>
        /// Called when the objective reaches its goal.
        /// Fires the <see cref="Completed"/> event and runs completion actions.
        /// </summary>
        protected virtual void OnComplete()
        {
            Completed?.Invoke(this);

            foreach (QuestActionCondition condition in OnCompleteActionsConditions)
            {
                if (!condition.CanExecute())
                {
                    return;
                }
            }

            foreach (QuestAction action in OnCompleteActions)
            {
                action.Execute(_quest);
            }
        }
    }
}
