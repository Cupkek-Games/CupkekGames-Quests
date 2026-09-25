using UnityEngine;

namespace CupkekGames.Quests
{
    /// <summary>An authored quest. Listed in a <see cref="QuestCatalog"/> and added to a log by its key.</summary>
    [CreateAssetMenu(fileName = "Quest", menuName = "CupkekGames/Quests/Quest")]
    public class QuestSO : ScriptableObject
    {
        [SerializeField] private QuestDefinition _definition = new QuestDefinition();
        public QuestDefinition Definition => _definition;
    }
}
