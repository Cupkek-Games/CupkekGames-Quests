using System;
using System.Collections.Generic;
using CupkekGames.Data;
using UnityEngine;

namespace CupkekGames.Quests
{
    /// <summary>
    /// Runtime quest instance — saved as part of player game state via <see cref="IData"/>.
    /// Cloned from a <see cref="QuestDefinition"/> when the player accepts a quest.
    /// Holds runtime fields (TimeLeft, IsCompleted, per-objective Progress) plus copies of
    /// the definition's authored data (Name, Description, action / condition / reward / feature lists).
    /// </summary>
    [Serializable]
    public class QuestInstance : IData
    {
        [SerializeField] private Guid _id;
        public Guid Id => _id;

        /// <summary>Catalog key of the source <see cref="QuestSO"/> — used to look up the original definition if needed.</summary>
        public string DefinitionKey;

        /// <summary>Authored, copied from definition at instantiation.</summary>
        public string Name;

        /// <summary>Authored, copied from definition at instantiation.</summary>
        [TextArea(3, 6)] public string Description;

        [SerializeField] private int _timeLeft;

        /// <summary>Remaining time units before the quest fails. -1 = no time limit.</summary>
        public int TimeLeft
        {
            get => _timeLeft;
            set => _timeLeft = value <= 0 ? -1 : value;
        }

        [SerializeField] private bool _isCompleted;
        public bool IsCompleted => _isCompleted;

        /// <summary>Runtime objective instances, cloned from the definition's templates with progress tracking.</summary>
        [SerializeReference] public List<QuestObjective> Objectives = new List<QuestObjective>();

        // Copied from definition at instantiation (so the runtime instance is self-contained for save/load):
        [SerializeReference] public List<QuestAction> OnStartActions = new List<QuestAction>();
        [SerializeReference] public List<QuestAction> OnReadyActions = new List<QuestAction>();
        [SerializeReference] public List<QuestAction> OnCompleteActions = new List<QuestAction>();
        [SerializeReference] public List<QuestActionCondition> OnStartActionsConditions = new List<QuestActionCondition>();
        [SerializeReference] public List<QuestActionCondition> OnReadyActionsConditions = new List<QuestActionCondition>();
        [SerializeReference] public List<QuestActionCondition> OnCompleteActionsConditions = new List<QuestActionCondition>();
        [SerializeReference] public List<IQuestRewardFeature> Rewards = new List<IQuestRewardFeature>();
        [SerializeReference] public List<IQuestFeature> Features = new List<IQuestFeature>();

        public QuestInstance()
        {
            _id = Guid.NewGuid();
        }

        public QuestInstance(Guid id)
        {
            _id = id;
        }

        /// <summary>Get the first feature of type <typeparamref name="T"/> on this quest, or null if none.</summary>
        public T GetFeature<T>() where T : class, IQuestFeature
        {
            for (int i = 0; i < Features.Count; i++)
            {
                if (Features[i] is T typed) return typed;
            }
            return null;
        }

        // ── Status ──────────────────────────────────────────────

        public QuestStatus GetStatus()
        {
            if (TimeLeft == 0) return QuestStatus.Failed;

            for (int i = 0; i < Objectives.Count; i++)
            {
                if (!Objectives[i].IsCompleted()) return QuestStatus.InProgress;
            }

            if (_isCompleted) return QuestStatus.Completed;

            return QuestStatus.Ready;
        }

        // ── Time ────────────────────────────────────────────────

        public void OnTimeProgress(int progressAmount)
        {
            if (TimeLeft > 0) TimeLeft -= progressAmount;
        }

        // ── Lifecycle ───────────────────────────────────────────

        public virtual void Activate()
        {
            foreach (QuestObjective objective in Objectives)
            {
                if (objective.IsCompleted()) continue;
                objective.Activate();
                if (objective.BlockNextObjective) break;
            }
        }

        public virtual void Deactivate()
        {
            foreach (QuestObjective objective in Objectives)
            {
                objective.Deactivate();
            }
        }

        public virtual void OnStart()
        {
            foreach (QuestActionCondition condition in OnStartActionsConditions)
            {
                if (!condition.CanExecute()) return;
            }

            foreach (QuestAction action in OnStartActions)
            {
                action.Execute(this);
            }

            OnProgress();
        }

        public virtual void OnProgress()
        {
            foreach (QuestObjective objective in Objectives)
            {
                if (objective.IsCompleted()) continue;
                objective.OnStart(this);
                if (objective.BlockNextObjective) break;
            }
        }

        public virtual void OnReady()
        {
            foreach (QuestActionCondition condition in OnReadyActionsConditions)
            {
                if (!condition.CanExecute()) return;
            }

            foreach (QuestAction action in OnReadyActions)
            {
                action.Execute(this);
            }
        }

        public virtual void Complete()
        {
            _isCompleted = true;

            foreach (IQuestRewardFeature reward in Rewards)
            {
                reward?.Apply();
            }

            foreach (QuestActionCondition condition in OnCompleteActionsConditions)
            {
                if (!condition.CanExecute()) return;
            }

            foreach (QuestAction action in OnCompleteActions)
            {
                action.Execute(this);
            }
        }

        // ── IData ───────────────────────────────────────────────

        public bool Validate() => true;

        public void OnAfterDeserialize() { }

        public IData CloneData()
        {
            QuestInstance clone = new QuestInstance(_id);
            clone.DefinitionKey = DefinitionKey;
            clone.Name = Name;
            clone.Description = Description;
            clone._timeLeft = _timeLeft;
            clone._isCompleted = _isCompleted;
            foreach (QuestObjective o in Objectives) clone.Objectives.Add(o?.Clone());
            foreach (QuestAction a in OnStartActions) clone.OnStartActions.Add(a);
            foreach (QuestAction a in OnReadyActions) clone.OnReadyActions.Add(a);
            foreach (QuestAction a in OnCompleteActions) clone.OnCompleteActions.Add(a);
            foreach (QuestActionCondition c in OnStartActionsConditions) clone.OnStartActionsConditions.Add(c);
            foreach (QuestActionCondition c in OnReadyActionsConditions) clone.OnReadyActionsConditions.Add(c);
            foreach (QuestActionCondition c in OnCompleteActionsConditions) clone.OnCompleteActionsConditions.Add(c);
            foreach (IQuestRewardFeature r in Rewards) clone.Rewards.Add(r?.CloneFeature());
            foreach (IQuestFeature f in Features) clone.Features.Add(f?.CloneFeature());
            return clone;
        }
    }
}
