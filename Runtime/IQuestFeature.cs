namespace CupkekGames.Quests
{
    /// <summary>
    /// Game data on a <see cref="QuestDefinition"/> (the giver, the source, a story line).
    /// The log never reads features; the game does, through <see cref="QuestDefinition.GetFeature{T}"/>.
    /// </summary>
    public interface IQuestFeature
    {
    }
}
