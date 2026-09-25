using CupkekGames.Data;

namespace CupkekGames.Quests
{
    /// <summary>
    /// Game data on a <see cref="QuestDefinition"/> (the giver, the source, how a template
    /// rolls). The log never reads features; the game does, through
    /// <see cref="QuestDefinition.GetFeature{T}"/>. Per-quest values a feature needs at
    /// runtime live on the <see cref="QuestState"/> as <see cref="IFeatureStateData"/>.
    /// </summary>
    public interface IQuestFeature : IFeature
    {
    }
}
