using System;
using System.Collections.Generic;
using UnityEngine;

namespace CupkekGames.Quests
{
  /// <summary>
  /// Runtime quest instance. Holds objectives, actions, conditions, reward, and lifecycle logic.
  /// Subclass this to add game-specific fields (e.g. client type, NPC references).
  /// <para>
  /// <b>Lifecycle:</b> <see cref="Activate"/> → <see cref="OnStart"/> → (objectives progress)
  /// → <see cref="OnProgress"/> → <see cref="OnReady"/> → <see cref="Complete"/> → <see cref="Deactivate"/>.
  /// </para>
  /// </summary>
  [Serializable]
  public class Quest
  {
    [SerializeField] private Guid _id;

    /// <summary>Unique identifier for this quest instance.</summary>
    public Guid Id => _id;

    /// <summary>Display name of the quest.</summary>
    public string Name;

    /// <summary>Quest description shown to the player.</summary>
    [Multiline(6)] public string Description;

    [SerializeField] private int _timeLeft;

    /// <summary>
    /// Remaining time units before the quest fails. Set to -1 for no time limit.
    /// Setting a value &lt;= 0 clamps to -1 (infinite).
    /// </summary>
    public int TimeLeft
    {
      get => _timeLeft;
      set => _timeLeft = value <= 0 ? -1 : value;
    }

    [SerializeField] private bool _isCompleted;

    /// <summary>Whether this quest has been turned in / completed.</summary>
    public bool IsCompleted => _isCompleted;

    /// <summary>Ordered list of objectives the player must complete.</summary>
    [SerializeReference] public List<QuestObjective> Objectives = new List<QuestObjective>();

    /// <summary>Reward granted when the quest is completed. May be null.</summary>
    [SerializeReference] public QuestReward Reward;

    /// <summary>Actions executed when the quest first starts.</summary>
    [SerializeReference] public List<QuestAction> OnStartActions = new List<QuestAction>();

    /// <summary>Actions executed when all objectives are done and the quest is ready to turn in.</summary>
    [SerializeReference] public List<QuestAction> OnReadyActions = new List<QuestAction>();

    /// <summary>Actions executed when the quest is completed.</summary>
    [SerializeReference] public List<QuestAction> OnCompleteActions = new List<QuestAction>();

    /// <summary>Conditions that must all pass before start actions execute.</summary>
    [SerializeReference] public List<QuestActionCondition> OnStartActionsConditions = new List<QuestActionCondition>();

    /// <summary>Conditions that must all pass before ready actions execute.</summary>
    [SerializeReference] public List<QuestActionCondition> OnReadyActionsConditions = new List<QuestActionCondition>();

    /// <summary>Conditions that must all pass before complete actions execute.</summary>
    [SerializeReference] public List<QuestActionCondition> OnCompleteActionsConditions = new List<QuestActionCondition>();

    /// <summary>Create a new quest with a random ID.</summary>
    public Quest()
    {
      _id = Guid.NewGuid();
    }

    /// <summary>Create a quest with a specific ID (used for cloning or deserialization).</summary>
    public Quest(Guid id)
    {
      _id = id;
    }

    // ── Status ──────────────────────────────────────────────

    /// <summary>
    /// Evaluate the current status of this quest based on time, objectives, and completion state.
    /// </summary>
    public QuestStatus GetStatus()
    {
      if (TimeLeft == 0)
      {
        return QuestStatus.Failed;
      }

      for (int i = 0; i < Objectives.Count; i++)
      {
        if (!Objectives[i].IsCompleted())
        {
          return QuestStatus.InProgress;
        }
      }

      if (_isCompleted)
      {
        return QuestStatus.Completed;
      }

      return QuestStatus.Ready;
    }

    // ── Time ────────────────────────────────────────────────

    /// <summary>Decrease remaining time. Does nothing if TimeLeft is already -1 (infinite).</summary>
    public void OnTimeProgress(int progressAmount)
    {
      if (TimeLeft > 0)
      {
        TimeLeft -= progressAmount;
      }
    }

    // ── Objectives ──────────────────────────────────────────

    /// <summary>Add an objective to this quest.</summary>
    public void AddObjective(QuestObjective objective)
    {
      Objectives.Add(objective);
    }

    // ── Lifecycle ───────────────────────────────────────────

    /// <summary>
    /// Activate all non-completed objectives for event listening.
    /// Respects <see cref="QuestObjective.BlockNextObjective"/> ordering.
    /// </summary>
    public virtual void Activate()
    {
      foreach (QuestObjective objective in Objectives)
      {
        if (objective.IsCompleted())
        {
          continue;
        }

        objective.Activate();

        if (objective.BlockNextObjective)
        {
          break;
        }
      }
    }

    /// <summary>Deactivate all objectives (unsubscribe from events).</summary>
    public virtual void Deactivate()
    {
      foreach (QuestObjective objective in Objectives)
      {
        objective.Deactivate();
      }
    }

    /// <summary>
    /// Called when the quest first starts. Runs start actions (if conditions pass), then
    /// calls <see cref="OnProgress"/> to activate the first objective.
    /// </summary>
    public virtual void OnStart()
    {
      foreach (QuestActionCondition condition in OnStartActionsConditions)
      {
        if (!condition.CanExecute())
        {
          return;
        }
      }

      foreach (QuestAction action in OnStartActions)
      {
        action.Execute(this);
      }

      OnProgress();
    }

    /// <summary>
    /// Called when an objective completes but the quest is not yet fully ready.
    /// Finds the next uncompleted objective and calls its <see cref="QuestObjective.OnStart"/>.
    /// </summary>
    public virtual void OnProgress()
    {
      foreach (QuestObjective objective in Objectives)
      {
        if (objective.IsCompleted())
        {
          continue;
        }

        objective.OnStart(this);

        if (objective.BlockNextObjective)
        {
          break;
        }
      }
    }

    /// <summary>
    /// Called when all objectives are complete and the quest is ready to turn in.
    /// Runs ready actions if all conditions pass.
    /// </summary>
    public virtual void OnReady()
    {
      foreach (QuestActionCondition condition in OnReadyActionsConditions)
      {
        if (!condition.CanExecute())
        {
          return;
        }
      }

      foreach (QuestAction action in OnReadyActions)
      {
        action.Execute(this);
      }
    }

    /// <summary>
    /// Mark the quest as completed. Applies the reward and runs complete actions if conditions pass.
    /// </summary>
    public virtual void Complete()
    {
      _isCompleted = true;

      Reward?.Apply();

      foreach (QuestActionCondition condition in OnCompleteActionsConditions)
      {
        if (!condition.CanExecute())
        {
          return;
        }
      }

      foreach (QuestAction action in OnCompleteActions)
      {
        action.Execute(this);
      }
    }

    // ── Clone ───────────────────────────────────────────────

    /// <summary>
    /// Create a shallow clone of this quest's template data (name, description, time, reward).
    /// The clone gets empty objective/action/condition lists — those are populated by
    /// <see cref="QuestSO.CreateInstance"/> from ScriptableObject definitions.
    /// Override in subclasses to copy game-specific fields.
    /// </summary>
    /// <param name="id">If provided, the clone uses this ID; otherwise a new ID is generated.</param>
    public virtual Quest Clone(Guid? id)
    {
      Quest clone = id.HasValue ? new Quest(id.Value) : new Quest();
      CopyCoreTo(clone);
      return clone;
    }

    /// <summary>
    /// Copy core template data to another quest instance.
    /// Subclasses should call <c>base.CopyCoreTo()</c> then copy their own fields.
    /// Note: Lists are new instances but contain the same element references (shallow copy).
    /// </summary>
    protected void CopyCoreTo(Quest target)
    {
      target.Name = Name;
      target.Description = Description;
      target.TimeLeft = TimeLeft;
      target._isCompleted = _isCompleted;
      target.Reward = Reward;
    }
  }
}
