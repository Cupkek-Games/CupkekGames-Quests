using System;

namespace CupkekGames.Quests
{
    /// <summary>
    /// Abstract base class for quest rewards.
    /// Implement game-specific reward logic in a subclass.
    /// </summary>
    [Serializable]
    public abstract class QuestReward
    {
        /// <summary>
        /// Apply this reward. Implementation is game-specific.
        /// </summary>
        public abstract void Apply();

        /// <summary>
        /// Get a human-readable description of the reward.
        /// </summary>
        public abstract string GetDescription();
    }
}
