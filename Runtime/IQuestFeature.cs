namespace CupkekGames.Quests
{
    /// <summary>
    /// Marker for game-specific quest extension data (e.g. NPC client reference, quest type, region).
    /// Domain games author concrete <c>IQuestFeature</c> impls and attach them to a <see cref="QuestDefinition.Features"/>
    /// list — replaces subclassing <see cref="QuestInstance"/> for variant data.
    /// Features clone with their containing definition (no per-instance state).
    /// </summary>
    public interface IQuestFeature
    {
        /// <summary>Deep copy when the containing quest is cloned into a runtime instance.</summary>
        IQuestFeature CloneFeature();
    }
}
