using System;
using System.Collections.Generic;
using CupkekGames.Data;

namespace CupkekGames.Quests
{
    /// <summary>
    /// What a generated quest rolled from its template: per objective, the key and the
    /// required count that replace the definition's (an empty key or a count of 0 keeps
    /// the definition's). Saved on the <see cref="QuestState"/>; the definition stays the
    /// template asset.
    /// </summary>
    [Serializable]
    public sealed class QuestRoll : IFeatureStateData
    {
        public List<string> Keys = new List<string>();
        public List<int> Required = new List<int>();

        public QuestRoll() { }

        /// <summary>Sets objective <paramref name="index"/>'s rolled key and count.</summary>
        public void Set(int index, string key, int required)
        {
            while (Keys.Count <= index) Keys.Add(null);
            while (Required.Count <= index) Required.Add(0);
            Keys[index] = key;
            Required[index] = required;
        }

        public string KeyAt(int index) => index < Keys.Count ? Keys[index] : null;
        public int RequiredAt(int index) => index < Required.Count ? Required[index] : 0;

        public IFeatureStateData CloneState() => new QuestRoll
        {
            Keys = new List<string>(Keys),
            Required = new List<int>(Required),
        };
    }
}
