using UnityEngine;

namespace CupkekGames.Quests
{
    /// <summary>
    /// Abstract ScriptableObject factory for creating QuestActionCondition runtime instances.
    /// </summary>
    public abstract class QuestActionConditionSO : ScriptableObject
    {
        public abstract QuestActionCondition CreateInstance();
    }
}
