using UnityEngine;

namespace CupkekGames.Quests
{
    /// <summary>
    /// Abstract ScriptableObject factory for creating QuestAction runtime instances.
    /// </summary>
    public abstract class QuestActionSO : ScriptableObject
    {
        public abstract QuestAction CreateInstance();
    }
}
