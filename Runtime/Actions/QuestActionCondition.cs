using System;

namespace CupkekGames.Quests
{
    /// <summary>
    /// Abstract base class for quest action conditions.
    /// Conditions gate whether a set of actions should execute.
    /// </summary>
    [Serializable]
    public abstract class QuestActionCondition
    {
        /// <summary>
        /// Evaluate whether the condition is met.
        /// </summary>
        /// <returns>True if the associated actions should execute.</returns>
        public abstract bool CanExecute();
    }
}
