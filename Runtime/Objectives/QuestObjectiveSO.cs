using System;
using System.Collections.Generic;
using UnityEngine;

namespace CupkekGames.Quests
{
  /// <summary>
  /// Abstract ScriptableObject factory for creating QuestObjective runtime instances.
  /// Subclass this for each objective type and assign action/condition SOs in the inspector.
  /// </summary>
  public abstract class QuestObjectiveSO : ScriptableObject
  {
    public bool BlockNextObjective;

    [Header("Objective Actions")]
    public List<QuestActionSO> OnStartActions = new List<QuestActionSO>();
    public List<QuestActionSO> OnCompleteActions = new List<QuestActionSO>();

    [Header("Objective Conditions")]
    public List<QuestActionConditionSO> OnStartActionsConditions = new List<QuestActionConditionSO>();
    public List<QuestActionConditionSO> OnCompleteActionsConditions = new List<QuestActionConditionSO>();

    /// <summary>
    /// Create a runtime QuestObjective instance for the given quest.
    /// </summary>
    public abstract QuestObjective CreateInstance(Guid questId);

    /// <summary>
    /// Populate the objective instance's action and condition lists from this SO's configuration.
    /// Call this from your CreateInstance override after creating the objective.
    /// </summary>
    protected void PopulateActions(QuestObjective instance)
    {
      foreach (QuestActionSO actionSO in OnStartActions)
      {
        instance.OnStartActions.Add(actionSO.CreateInstance());
      }

      foreach (QuestActionSO actionSO in OnCompleteActions)
      {
        instance.OnCompleteActions.Add(actionSO.CreateInstance());
      }

      foreach (QuestActionConditionSO conditionSO in OnStartActionsConditions)
      {
        instance.OnStartActionsConditions.Add(conditionSO.CreateInstance());
      }

      foreach (QuestActionConditionSO conditionSO in OnCompleteActionsConditions)
      {
        instance.OnCompleteActionsConditions.Add(conditionSO.CreateInstance());
      }
    }
  }
}
