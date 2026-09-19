using System;
using System.Collections.Generic;
using MindSilence.Data;
using MindSilence.Domain;
using NUnit.Framework;

namespace MindSilence.Tests.EditMode
{
    public sealed class GameSessionTests
    {
        [Test]
        public void Start_TransitionsToRunningLevelOne()
        {
            using var session = CreateSession(out _);

            session.Start();

            var state = session.State;
            Assert.AreEqual(GamePhase.Running, state.Phase);
            Assert.AreEqual(1, state.Level);
            Assert.AreEqual(0, state.ElapsedSecAtLevel);
            Assert.AreEqual(4, state.RequiredSecAtLevel);
        }

        [Test]
        public void FourSecondsOfSilence_AdvancesToLevelTwo()
        {
            using var session = CreateSession(out var time);
            session.Start();

            time.Advance(TimeSpan.FromSeconds(4));

            Assert.AreEqual(2, session.State.Level);
            Assert.AreEqual(0, session.State.ElapsedSecAtLevel);
        }

        [Test]
        public void EightSecondsOnLevelTwo_AdvancesToLevelThree()
        {
            using var session = CreateSession(out var time);
            session.Start();

            time.Advance(TimeSpan.FromSeconds(4));
            time.Advance(TimeSpan.FromSeconds(8));

            Assert.AreEqual(3, session.State.Level);
        }

        [Test]
        public void Thought_ResetsToIdleAndShowsSessionSummary()
        {
            using var session = CreateSession(out var time);
            session.Start();
            time.Advance(TimeSpan.FromMilliseconds(500));

            session.Thought();

            var state = session.State;
            Assert.AreEqual(GamePhase.Idle, state.Phase);
            Assert.AreEqual(0, state.Level);
            Assert.AreEqual(0, state.ElapsedSecAtLevel);
            Assert.AreEqual(new SessionSummary(1, 1, 0), state.SessionSummary);
        }

        [Test]
        public void Thought_KeepsBestTodayAcrossSessions()
        {
            var repository = new InMemoryGameProgressRepository();
            using var session = CreateSession(out var time, repository);

            session.Start();
            time.Advance(TimeSpan.FromSeconds(4));
            session.Thought();
            session.DismissSessionSummary();

            session.Start();
            session.Thought();

            Assert.AreEqual(new SessionSummary(1, 2, 0), session.State.SessionSummary);
        }

        [Test]
        public void DismissSessionSummary_ClearsDialogState()
        {
            using var session = CreateSession(out _);
            session.Start();
            session.Thought();

            session.DismissSessionSummary();

            Assert.IsNull(session.State.SessionSummary);
        }

        [Test]
        public void OpenHighScores_ClearsSummaryAndNavigates()
        {
            using var session = CreateSession(out _);
            var navigated = false;
            session.NavigateToHighScores += () => navigated = true;
            session.Start();
            session.Thought();

            session.OpenHighScores();

            Assert.IsNull(session.State.SessionSummary);
            Assert.IsTrue(navigated);
        }

        [Test]
        public void LeaveTraining_EmitsNavigateBackToMenu()
        {
            using var session = CreateSession(out _);
            var navigated = false;
            session.NavigateBackToMenu += () => navigated = true;

            session.LeaveTraining();

            Assert.IsTrue(navigated);
        }

        [Test]
        public void ThoughtInIdle_IsNoOp()
        {
            using var session = CreateSession(out _);

            session.Thought();

            Assert.AreEqual(GamePhase.Idle, session.State.Phase);
            Assert.AreEqual(0, session.State.Level);
        }

        [Test]
        public void Thought_EmitsHaptic()
        {
            using var session = CreateSession(out _);
            var haptic = false;
            session.HapticOnThought += () => haptic = true;
            session.Start();

            session.Thought();

            Assert.IsTrue(haptic);
        }

        [Test]
        public void Thought_IncludesTotalSessionTimeInSummary()
        {
            using var session = CreateSession(out var time);
            session.Start();
            time.Advance(TimeSpan.FromSeconds(4));
            time.Advance(TimeSpan.FromSeconds(8));

            session.Thought();

            Assert.AreEqual(new SessionSummary(3, 3, 12), session.State.SessionSummary);
        }

        [Test]
        public void StartWhileRunning_IsNoOp()
        {
            using var session = CreateSession(out var time);
            session.Start();
            time.Advance(TimeSpan.FromSeconds(2));

            var elapsed = session.State.ElapsedSecAtLevel;
            session.Start();

            Assert.AreEqual(GamePhase.Running, session.State.Phase);
            Assert.AreEqual(1, session.State.Level);
            Assert.AreEqual(elapsed, session.State.ElapsedSecAtLevel);
        }

        [Test]
        public void Start_EmitsKeepAwakeTrue()
        {
            using var session = CreateSession(out _);
            bool? keepAwake = null;
            session.KeepAwake += enabled => keepAwake = enabled;

            session.Start();

            Assert.AreEqual(true, keepAwake);
        }

        [Test]
        public void BackgroundWhileIdle_IsNoOp()
        {
            using var session = CreateSession(out _);
            var before = session.State;

            session.AppBackgrounded();

            Assert.AreEqual(before, session.State);
        }

        [Test]
        public void BackgroundWhileRunning_PausesTickAndKeepsPhase()
        {
            using var session = CreateSession(out var time);
            var keepAwake = new List<bool>();
            session.KeepAwake += keepAwake.Add;
            session.Start();

            session.AppBackgrounded();

            Assert.AreEqual(false, keepAwake[keepAwake.Count - 1]);
            Assert.AreEqual(GamePhase.Running, session.State.Phase);
            var elapsed = session.State.ElapsedSecAtLevel;
            time.Advance(TimeSpan.FromSeconds(4));
            Assert.AreEqual(elapsed, session.State.ElapsedSecAtLevel);
        }

        [Test]
        public void Foreground_ResumesTickOnlyAfterRunningBackground()
        {
            using var session = CreateSession(out var time);
            var keepAwake = new List<bool>();
            session.KeepAwake += keepAwake.Add;
            session.Start();
            session.AppBackgrounded();

            session.AppForegrounded();

            Assert.AreEqual(true, keepAwake[keepAwake.Count - 1]);
            time.Advance(TimeSpan.FromSeconds(4));
            Assert.AreEqual(2, session.State.Level);
        }

        [Test]
        public void ForegroundWithoutBackground_DoesNotResetRunningSession()
        {
            using var session = CreateSession(out var time);
            session.Start();

            session.AppForegrounded();
            time.Advance(TimeSpan.FromSeconds(4));

            Assert.AreEqual(GamePhase.Running, session.State.Phase);
            Assert.AreEqual(2, session.State.Level);
        }

        [Test]
        public void IdleProgressFraction_IsZero()
        {
            var idle = new GameSessionState();
            Assert.AreEqual(0, idle.RequiredSecAtLevel);
            Assert.AreEqual(0f, idle.ProgressFraction, 0.0001f);
        }

        [Test]
        public void RunningProgressFraction_IsElapsedOverRequired()
        {
            var running = new GameSessionState(GamePhase.Running, level: 1, elapsedSecAtLevel: 2);
            Assert.AreEqual(4, running.RequiredSecAtLevel);
            Assert.AreEqual(0.5f, running.ProgressFraction, 0.0001f);
        }

        [Test]
        public void ProgressFraction_IsCoercedToOne()
        {
            var overflowing = new GameSessionState(GamePhase.Running, level: 1, elapsedSecAtLevel: 100);
            Assert.AreEqual(1f, overflowing.ProgressFraction, 0.0001f);
        }

        [Test]
        public void Dispose_StopsTicking()
        {
            var time = new FakeTimeSource();
            var session = new GameSession(new InMemoryGameProgressRepository(), time);
            session.Start();
            session.Dispose();

            var elapsed = session.State.ElapsedSecAtLevel;
            time.Advance(TimeSpan.FromSeconds(4));
            Assert.AreEqual(elapsed, session.State.ElapsedSecAtLevel);
        }

        [Test]
        public void Dispose_EmitsKeepAwakeFalse()
        {
            using var session = CreateSession(out _);
            bool? keepAwake = null;
            session.KeepAwake += enabled => keepAwake = enabled;
            session.Start();

            session.Dispose();

            Assert.AreEqual(false, keepAwake);
        }

        private static GameSession CreateSession(
            out FakeTimeSource time,
            IGameProgressRepository repository = null)
        {
            time = new FakeTimeSource();
            return new GameSession(repository ?? new InMemoryGameProgressRepository(), time);
        }
    }
}
