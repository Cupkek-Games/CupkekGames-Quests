namespace CupkekGames.Quests
{
    /// <summary>A reward the game grants when a quest is turned in (gold, an item, an unlock).</summary>
    public interface IQuestReward
    {
        void Grant();

        /// <summary>One line for the UI.</summary>
        string Describe();
    }
}
