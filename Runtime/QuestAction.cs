using System;

namespace CupkekGames.Quests
{
    /// <summary>
    /// Abstract base for quest actions. Authored as <c>[SerializeReference]</c> entries in the action lists
    /// on <see cref="QuestDefinition"/>. Concrete subclasses must provide a parameterless constructor.
    /// </summary>
    [Serializable]
    public abstract class QuestAction
    {
        protected QuestAction() { }

        public abstract void Execute(QuestInstance quest);
    }
}
