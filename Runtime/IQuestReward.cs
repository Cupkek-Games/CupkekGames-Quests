using CupkekGames.Data;

namespace CupkekGames.Quests
{
    /// <summary>
    /// A reward the game grants when a quest is turned in (gold, an item, an unlock). It is
    /// handed the quest, so a reward can read what the quest rolled or who it belongs to.
    /// </summary>
    public interface IQuestReward : IFeature
    {
        void Grant(QuestState quest);

        /// <summary>One line for the UI.</summary>
        string Describe(QuestState quest);
    }
}
