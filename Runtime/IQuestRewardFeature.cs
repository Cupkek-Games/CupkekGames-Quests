namespace CupkekGames.Quests
{
    /// <summary>
    /// Composable quest reward — multiple per quest (gold + items + XP + reputation stack via separate features).
    /// Replaces the single <c>QuestReward</c> abstract slot.
    /// </summary>
    public interface IQuestRewardFeature
    {
        /// <summary>Apply this reward. Domain-specific implementation.</summary>
        void Apply();

        /// <summary>Human-readable description shown in UI (single line per feature).</summary>
        string GetDescription();

        /// <summary>Deep copy when the containing quest is cloned into a runtime instance.</summary>
        IQuestRewardFeature CloneFeature();
    }
}
