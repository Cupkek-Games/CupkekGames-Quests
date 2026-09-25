using System;
using System.Collections.Generic;
using CupkekGames.Quests;
using NUnit.Framework;

namespace CupkekGames.Quests.Tests
{
    public class QuestLogTests
    {
        private class CountingReward : IQuestReward
        {
            public int Granted;
            public void Grant() => Granted++;
            public string Describe() => "count";
        }

        private static QuestDefinition Define(bool ordered, params QuestObjectiveDefinition[] objectives)
        {
            QuestDefinition definition = new QuestDefinition { Title = "Test", Ordered = ordered };
            definition.Objectives.AddRange(objectives);
            return definition;
        }

        private static QuestLog Bound(Dictionary<string, QuestDefinition> authored = null)
        {
            QuestLog log = new QuestLog();
            log.Bind(key => authored != null && authored.TryGetValue(key, out QuestDefinition d) ? d : null);
            return log;
        }

        [Test]
        public void Signal_MovesMatchingObjective_AndMarksReady()
        {
            QuestLog log = Bound();
            QuestState quest = log.Add(Define(false, new QuestObjectiveDefinition("cook", "stew", 2)));
            int ready = 0;
            log.Ready += _ => ready++;

            log.Signal("cook", "soup");
            log.Signal("brew", "stew");
            Assert.AreEqual(0, quest.Progress[0], "other keys and kinds do not count");

            log.Signal("cook", "stew");
            Assert.AreEqual(QuestStatus.Active, quest.Status);
            log.Signal("cook", "stew", 5);
            Assert.AreEqual(2, quest.Progress[0], "progress stops at the required count");
            Assert.AreEqual(QuestStatus.Ready, quest.Status);
            Assert.AreEqual(1, ready);
        }

        [Test]
        public void EmptyKey_AcceptsAnyKey()
        {
            QuestLog log = Bound();
            QuestState quest = log.Add(Define(false, new QuestObjectiveDefinition("hire")));
            log.Signal("hire", "elaine");
            Assert.AreEqual(QuestStatus.Ready, quest.Status);
        }

        [Test]
        public void Ordered_OnlyFirstOpenObjectiveTakesProgress()
        {
            QuestLog log = Bound();
            QuestState quest = log.Add(Define(true,
                new QuestObjectiveDefinition("hire"),
                new QuestObjectiveDefinition("cook")));

            log.Signal("cook");
            Assert.AreEqual(0, quest.Progress[1], "the second objective waits for the first");
            log.Signal("hire");
            log.Signal("cook");
            Assert.AreEqual(QuestStatus.Ready, quest.Status);
        }

        [Test]
        public void Ordered_OneSignalMovesOneObjective()
        {
            QuestLog log = Bound();
            QuestState quest = log.Add(Define(true,
                new QuestObjectiveDefinition("node", "camp"),
                new QuestObjectiveDefinition("node", "camp")));
            log.Signal("node", "camp");
            Assert.AreEqual(1, quest.Progress[0]);
            Assert.AreEqual(0, quest.Progress[1]);
        }

        [Test]
        public void Report_RisesToValue_NeverFalls()
        {
            QuestLog log = Bound();
            QuestState quest = log.Add(Define(false, new QuestObjectiveDefinition("fame", null, 5)));
            log.Report("fame", null, 3);
            Assert.AreEqual(3, quest.Progress[0]);
            log.Report("fame", null, 1);
            Assert.AreEqual(3, quest.Progress[0]);
            log.Report("fame", null, 9);
            Assert.AreEqual(5, quest.Progress[0]);
            Assert.AreEqual(QuestStatus.Ready, quest.Status);
        }

        [Test]
        public void TurnIn_GrantsRewardsOnce_AndRefusesUnready()
        {
            QuestLog log = Bound();
            CountingReward reward = new CountingReward();
            QuestDefinition definition = Define(false, new QuestObjectiveDefinition("cook"));
            definition.Rewards.Add(reward);
            QuestState quest = log.Add(definition);

            Assert.Throws<InvalidOperationException>(() => log.TurnIn(quest));
            log.Signal("cook");
            log.TurnIn(quest);
            Assert.AreEqual(QuestStatus.Completed, quest.Status);
            Assert.AreEqual(1, reward.Granted);
            Assert.Throws<InvalidOperationException>(() => log.TurnIn(quest));

            log.Signal("cook");
            Assert.AreEqual(1, reward.Granted, "a completed quest takes no more progress");
        }

        [Test]
        public void NoObjectives_IsReadyOnAdd()
        {
            QuestLog log = Bound();
            QuestState quest = log.Add(Define(false));
            Assert.AreEqual(QuestStatus.Ready, quest.Status);
        }

        [Test]
        public void Tick_RunsDeadlineDown_AndFails()
        {
            QuestLog log = Bound();
            QuestState timed = log.Add(Define(false, new QuestObjectiveDefinition("cook")), deadline: 3);
            QuestState open = log.Add(Define(false, new QuestObjectiveDefinition("cook")));
            int failed = 0;
            log.Failed += _ => failed++;

            log.Tick(2);
            Assert.AreEqual(1, timed.Deadline);
            log.Tick(5);
            Assert.AreEqual(QuestStatus.Failed, timed.Status);
            Assert.AreEqual(QuestStatus.Active, open.Status, "no deadline, never fails");
            Assert.AreEqual(1, failed);

            log.Signal("cook");
            Assert.AreEqual(0, timed.Progress[0], "a failed quest takes no progress");
        }

        [Test]
        public void Authored_ResolvesByKey_AndUnknownKeyThrows()
        {
            QuestDefinition story = Define(false, new QuestObjectiveDefinition("hire"));
            QuestLog log = Bound(new Dictionary<string, QuestDefinition> { { "story_01", story } });

            QuestState quest = log.Add("story_01");
            Assert.AreSame(story, quest.Definition);
            Assert.IsNull(quest.Inline, "authored quests save only their key");
            Assert.AreSame(quest, log.FindByKey("story_01"));

            Assert.Throws<InvalidOperationException>(() => log.Add("missing"));
        }

        [Test]
        public void Clone_CopiesState_NotLinked()
        {
            QuestLog log = Bound();
            QuestState quest = log.Add(Define(false, new QuestObjectiveDefinition("cook", null, 3)));
            log.Signal("cook");

            QuestLog clone = (QuestLog)log.CloneData();
            log.Signal("cook");

            QuestState copied = clone.Find(quest.Id);
            Assert.AreEqual(1, copied.Progress[0]);
            Assert.AreEqual(2, quest.Progress[0]);
            Assert.AreSame(quest.Definition, copied.Definition);
        }

        [Test]
        public void Handler_AddingQuestDuringSignal_IsSafe()
        {
            QuestLog log = Bound();
            log.Add(Define(false, new QuestObjectiveDefinition("cook")));
            log.Ready += _ => log.Add(Define(false, new QuestObjectiveDefinition("cook")));

            Assert.DoesNotThrow(() => log.Signal("cook"));
            Assert.AreEqual(2, log.Quests.Count);
            Assert.AreEqual(QuestStatus.Active, log.Quests[1].Status, "the new quest did not take the signal that made it");
        }
    }
}
