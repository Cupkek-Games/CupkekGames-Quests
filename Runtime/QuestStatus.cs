namespace CupkekGames.Quests
{
    /// <summary>Where a quest stands in its log.</summary>
    public enum QuestStatus
    {
        /// <summary>Some objective is still open.</summary>
        Active,
        /// <summary>Every objective is done; the quest waits for <see cref="QuestLog.TurnIn"/>.</summary>
        Ready,
        /// <summary>Turned in; its rewards were granted.</summary>
        Completed,
        /// <summary>Failed: its deadline ran out, or the game failed it.</summary>
        Failed,
    }
}
