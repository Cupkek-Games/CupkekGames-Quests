using System;

namespace CupkekGames.Quests
{
    /// <summary>
    /// What rewards are granted into (the player's wallet, inventory, settings). The game
    /// implements it and passes it to <see cref="QuestLog.TurnIn"/>; each reward reads the
    /// game's type through <see cref="QuestGrantContextExtensions.Require{T}"/>.
    /// </summary>
    public interface IQuestGrantContext
    {
    }

    public static class QuestGrantContextExtensions
    {
        /// <summary>The context as the game's type; throws when it is missing or another type.</summary>
        public static T Require<T>(this IQuestGrantContext context) where T : class, IQuestGrantContext
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            return context as T ?? throw new InvalidOperationException(
                $"A quest reward needs a {typeof(T).Name} grant context, but was given a {context.GetType().Name}.");
        }
    }
}
