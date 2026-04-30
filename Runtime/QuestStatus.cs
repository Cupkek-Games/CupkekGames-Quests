namespace CupkekGames.Quests
{
    /// <summary>Represents the current state of a quest.</summary>
    public enum QuestStatus
    {
        /// <summary>The quest has uncompleted objectives.</summary>
        InProgress,
        /// <summary>All objectives are complete; the quest can be turned in.</summary>
        Ready,
        /// <summary>The quest has been turned in / completed.</summary>
        Completed,
        /// <summary>The quest failed (e.g. time ran out).</summary>
        Failed,
    }
}
