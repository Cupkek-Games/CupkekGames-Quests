using System;

namespace CupkekGames.Quests
{
    /// <summary>
    /// Abstract base for quest action conditions (gates whether a set of actions executes).
    /// Authored as <c>[SerializeReference]</c> entries in the condition lists on <see cref="QuestDefinition"/>.
    /// </summary>
    [Serializable]
    public abstract class QuestActionCondition
    {
        protected QuestActionCondition() { }

        public abstract bool CanExecute();
    }
}
