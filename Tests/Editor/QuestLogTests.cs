using System;
using System.Collections.Generic;
using CupkekGames.Data;
using CupkekGames.Quests;
using NUnit.Framework;

namespace CupkekGames.Quests.Tests
{
    public class QuestLogTests
    {
        private class CountingReward : IQuestReward
        {
            public int Granted;
            public QuestState LastQuest;
            public void Grant(QuestState quest) { Granted++; LastQuest = quest; }
            public string Describe(QuestState quest) => "count";
            public IFeature CloneFeature() => new CountingReward();
        }

        private class Marker : IFeatureStateData
        {
            public int Value;
            public IFeatureStateData CloneState() => new Marker { Value = Value };
        }

        private Dictionary<string, QuestDefinition> _definitions;
        private QuestLog _log;

        [SetUp]
        public void SetUp()
        {
            _definitions = new Dictionary<string, QuestDefinition>();
            _log = new QuestLog();
            _log.Bind(key => _definitions.TryGetValue(key.Key, out QuestDefinition d) ? d : null);
        }

        private static CatalogKey Key(string key) => new CatalogKey { Catalog = "Quests", Key = key };

        private CatalogKey Define(string key, bool ordered, params QuestObjectiveDefinition[] objectives)
        {
            QuestDefinition definition = new QuestDefinition { Title = key, Ordered = ordered };
            definition.Objectives.AddRange(objectives);
            _definitions[key] = definition;
            return Key(key);
        }

        [Test]
        public void Signal_MovesMatchingObjective_AndMarksReady()
        {
            QuestState quest = _log.Add(Define("cook", false, new QuestObjectiveDefinition("cook", "stew", 2)));
            int ready = 0;
            _log.Ready += _ => ready++;

            _log.Signal("cook", "soup");
            _log.Signal("brew", "stew");
            Assert.AreEqual(0, quest.Progress[0], "other keys and kinds do not count");

            _log.Signal("cook", "stew");
            Assert.AreEqual(QuestStatus.Active, quest.Status);
            _log.Signal("cook", "stew", 5);
            Assert.AreEqual(2, quest.Progress[0], "progress stops at the required count");
            Assert.AreEqual(QuestStatus.Ready, quest.Status);
            Assert.AreEqual(1, ready);
        }

        [Test]
        public void EmptyKey_AcceptsAnyKey()
        {
            QuestState quest = _log.Add(Define("hire", false, new QuestObjectiveDefinition("hire")));
            _log.Signal("hire", "elaine");
            Assert.AreEqual(QuestStatus.Ready, quest.Status);
        }

        [Test]
        public void Ordered_OnlyFirstOpenObjectiveTakesProgress()
        {
            QuestState quest = _log.Add(Define("chain", true,
                new QuestObjectiveDefinition("hire"),
                new QuestObjectiveDefinition("cook")));

            _log.Signal("cook");
            Assert.AreEqual(0, quest.Progress[1], "the second objective waits for the first");
            _log.Signal("hire");
            _log.Signal("cook");
            Assert.AreEqual(QuestStatus.Ready, quest.Status);
        }

        [Test]
        public void Ordered_OneSignalMovesOneObjective()
        {
            QuestState quest = _log.Add(Define("camps", true,
                new QuestObjectiveDefinition("node", "camp"),
                new QuestObjectiveDefinition("node", "camp")));
            _log.Signal("node", "camp");
            Assert.AreEqual(1, quest.Progress[0]);
            Assert.AreEqual(0, quest.Progress[1]);
        }

        [Test]
        public void Report_RisesToValue_NeverFalls()
        {
            QuestState quest = _log.Add(Define("fame", false, new QuestObjectiveDefinition("fame", null, 5)));
            _log.Report("fame", null, 3);
            Assert.AreEqual(3, quest.Progress[0]);
            _log.Report("fame", null, 1);
            Assert.AreEqual(3, quest.Progress[0]);
            _log.Report("fame", null, 9);
            Assert.AreEqual(5, quest.Progress[0]);
            Assert.AreEqual(QuestStatus.Ready, quest.Status);
        }

        [Test]
        public void TurnIn_GrantsRewardsOnce_WithTheQuest()
        {
            CatalogKey key = Define("paid", false, new QuestObjectiveDefinition("cook"));
            CountingReward reward = new CountingReward();
            _definitions["paid"].Rewards.Add(reward);
            QuestState quest = _log.Add(key);

            Assert.Throws<InvalidOperationException>(() => _log.TurnIn(quest));
            _log.Signal("cook");
            _log.TurnIn(quest);
            Assert.AreEqual(QuestStatus.Completed, quest.Status);
            Assert.AreEqual(1, reward.Granted);
            Assert.AreSame(quest, reward.LastQuest, "a reward is handed the quest it pays for");
            Assert.Throws<InvalidOperationException>(() => _log.TurnIn(quest));

            _log.Signal("cook");
            Assert.AreEqual(1, reward.Granted, "a completed quest takes no more progress");
        }

        [Test]
        public void NoObjectives_IsReadyOnAdd()
        {
            QuestState quest = _log.Add(Define("empty", false));
            Assert.AreEqual(QuestStatus.Ready, quest.Status);
        }

        [Test]
        public void Tick_RunsDeadlineDown_AndFails()
        {
            CatalogKey key = Define("timed", false, new QuestObjectiveDefinition("cook"));
            QuestState timed = _log.Add(key, deadline: 3);
            QuestState open = _log.Add(key);
            int failed = 0;
            _log.Failed += _ => failed++;

            _log.Tick(2);
            Assert.AreEqual(1, timed.Deadline);
            _log.Tick(5);
            Assert.AreEqual(QuestStatus.Failed, timed.Status);
            Assert.AreEqual(QuestStatus.Active, open.Status, "no deadline, never fails");
            Assert.AreEqual(1, failed);

            _log.Signal("cook");
            Assert.AreEqual(0, timed.Progress[0], "a failed quest takes no progress");
        }

        [Test]
        public void UnknownKey_Throws()
        {
            Assert.Throws<InvalidOperationException>(() => _log.Add(Key("missing")));
        }

        [Test]
        public void Roll_ReplacesKeyAndCount_PerQuest()
        {
            CatalogKey template = Define("contract", false, new QuestObjectiveDefinition("node", null, 1));
            QuestState camps = new QuestState(template);
            camps.GetOrCreateState<QuestRoll>().Set(0, "camp", 3);
            _log.Add(camps);
            QuestState plain = _log.Add(template);

            _log.Signal("node", "shrine");
            Assert.AreEqual(0, camps.Progress[0], "the rolled key narrows the objective");
            Assert.AreEqual(QuestStatus.Ready, plain.Status, "the template itself still accepts any node");

            _log.Signal("node", "camp", 2);
            Assert.AreEqual(QuestStatus.Active, camps.Status, "the rolled count is 3, not the template's 1");
            _log.Signal("node", "camp");
            Assert.AreEqual(QuestStatus.Ready, camps.Status);
        }

        [Test]
        public void Attach_ShowsAQuestWithoutAddingIt()
        {
            QuestState offer = new QuestState(Define("offer", false, new QuestObjectiveDefinition("cook")));
            _log.Attach(offer);
            Assert.AreEqual("offer", offer.Definition.Title);
            Assert.AreEqual(0, _log.Quests.Count);
            _log.Add(offer);
            Assert.Throws<InvalidOperationException>(() => _log.Add(offer), "a quest is added once");
        }

        [Test]
        public void Clone_CopiesStateAndFeatureState_NotLinked()
        {
            QuestState quest = _log.Add(Define("three", false, new QuestObjectiveDefinition("cook", null, 3)));
            quest.GetOrCreateState<Marker>().Value = 7;
            _log.Signal("cook");

            QuestLog clone = (QuestLog)_log.CloneData();
            _log.Signal("cook");
            quest.GetState<Marker>().Value = 8;

            QuestState copied = clone.Find(quest.Id);
            Assert.AreEqual(1, copied.Progress[0]);
            Assert.AreEqual(2, quest.Progress[0]);
            Assert.AreEqual(7, copied.GetState<Marker>().Value);
            Assert.AreSame(quest.Definition, copied.Definition);
        }

        [Test]
        public void Handler_AddingQuestDuringSignal_IsSafe()
        {
            CatalogKey key = Define("cook", false, new QuestObjectiveDefinition("cook"));
            _log.Add(key);
            _log.Ready += _ => _log.Add(key);

            Assert.DoesNotThrow(() => _log.Signal("cook"));
            Assert.AreEqual(2, _log.Quests.Count);
            Assert.AreEqual(QuestStatus.Active, _log.Quests[1].Status, "the new quest did not take the signal that made it");
        }
    }
}
