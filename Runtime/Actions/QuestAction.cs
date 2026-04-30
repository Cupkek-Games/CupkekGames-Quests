using System;

namespace CupkekGames.Quests
{
    /// <summary>
    /// Abstract base class for quest actions.
    /// Actions are executed at specific points in the quest lifecycle
    /// (on start, when ready, on complete) and on objectives.
    /// </summary>
    [Serializable]
    public abstract class QuestAction
    {
        /// <summary>
        /// Execute this action.
        /// </summary>
        /// <param name="quest">The quest this action belongs to.</param>
        public abstract void Execute(Quest quest);
    }
}
