using CupkekGames.KeyValueDatabases;

namespace CupkekGames.Quests
{
    /// <summary>
    /// MonoBehaviour database that maps string keys to QuestSO definitions.
    /// </summary>
    public class QuestDatabase : KeyValueDatabaseMonoSO<string, QuestSO>
    {
    }
}
