using System;
using System.Collections.Generic;
using UnityEngine;

namespace CupkekGames.Quests
{
  /// <summary>
  /// ScriptableObject definition for a quest.
  /// Contains the quest template and lists of objective/action/condition SOs.
  /// Use CreateInstance() to produce a runtime Quest.
  /// </summary>
  [CreateAssetMenu(fileName = "Quest", menuName = "CupkekGames/QuestSystem/Quest")]
  public class QuestSO : ScriptableObject
  {
    /// <summary>Template quest data. Cloned by <see cref="CreateInstance"/>.</summary>
    [SerializeReference] public Quest Quest;

    /// <summary>Objective definitions that will be instantiated for each runtime quest.</summary>
    [Header("Objectives")]
    public List<QuestObjectiveSO> Objectives = new List<QuestObjectiveSO>();

    /// <summary>Action definitions executed when the quest starts.</summary>
    [Header("Actions")]
    public List<QuestActionSO> OnStartActions = new List<QuestActionSO>();
    /// <summary>Action definitions executed when the quest is ready to turn in.</summary>
    public List<QuestActionSO> OnReadyActions = new List<QuestActionSO>();
    /// <summary>Action definitions executed when the quest completes.</summary>
    public List<QuestActionSO> OnCompleteActions = new List<QuestActionSO>();

    /// <summary>Conditions gating start actions.</summary>
    [Header("Action Conditions")]
    public List<QuestActionConditionSO> OnStartActionsConditions = new List<QuestActionConditionSO>();
    /// <summary>Conditions gating ready actions.</summary>
    public List<QuestActionConditionSO> OnReadyActionsConditions = new List<QuestActionConditionSO>();
    /// <summary>Conditions gating complete actions.</summary>
    public List<QuestActionConditionSO> OnCompleteActionsConditions = new List<QuestActionConditionSO>();

    /// <summary>
    /// Create a runtime Quest instance from this definition.
    /// Override in subclasses to produce game-specific Quest subclasses.
    /// </summary>
    public virtual Quest CreateInstance()
    {
      Quest clone = Quest.Clone(Quest.Id);

      foreach (QuestObjectiveSO objectiveSO in Objectives)
      {
        clone.AddObjective(objectiveSO.CreateInstance(clone.Id));
      }

      foreach (QuestActionSO actionSO in OnStartActions)
      {
        clone.OnStartActions.Add(actionSO.CreateInstance());
      }

      foreach (QuestActionSO actionSO in OnReadyActions)
      {
        clone.OnReadyActions.Add(actionSO.CreateInstance());
      }

      foreach (QuestActionSO actionSO in OnCompleteActions)
      {
        clone.OnCompleteActions.Add(actionSO.CreateInstance());
      }

      foreach (QuestActionConditionSO conditionSO in OnStartActionsConditions)
      {
        clone.OnStartActionsConditions.Add(conditionSO.CreateInstance());
      }

      foreach (QuestActionConditionSO conditionSO in OnReadyActionsConditions)
      {
        clone.OnReadyActionsConditions.Add(conditionSO.CreateInstance());
      }

      foreach (QuestActionConditionSO conditionSO in OnCompleteActionsConditions)
      {
        clone.OnCompleteActionsConditions.Add(conditionSO.CreateInstance());
      }

      return clone;
    }
  }
}
