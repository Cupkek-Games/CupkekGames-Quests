using CupkekGames.Data;
using UnityEngine;

namespace CupkekGames.Quests
{
    /// <summary>The authored quests, by key. A log resolves saved keys through it (<see cref="QuestLog.Bind"/>).</summary>
    [CreateAssetMenu(fileName = "QuestCatalog", menuName = "CupkekGames/Quests/Quest Catalog")]
    public class QuestCatalog : AssetCatalog<QuestSO>
    {
    }
}
