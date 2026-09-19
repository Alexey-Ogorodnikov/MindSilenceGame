using System;
using MindSilence.Data;
using MindSilence.Domain;
using MindSilence.Presentation;
using MindSilence.XR;
using NUnit.Framework;
using UnityEngine;

namespace MindSilence.Tests.EditMode
{
    public sealed class LifecycleBridgeTests
    {
        GameObject _host;

        [SetUp]
        public void SetUp()
        {
            _host = new GameObject("LifecycleHost");
        }

        [TearDown]
        public void TearDown()
        {
            if (_host != null)
            {
                UnityEngine.Object.DestroyImmediate(_host);
            }

            KeepAwakeBridge.SetEnabled(false);
        }

        [Test]
        public void PauseTrue_StopsTick_KeepsRunningPhase()
        {
            var time = new FakeTimeSource();
            using var session = new GameSession(new InMemoryGameProgressRepository(), time);
            var bridge = _host.AddComponent<LifecycleBridge>();
            bridge.ApplicationPaused += paused =>
            {
                if (paused)
                {
                    session.AppBackgrounded();
                }
                else
                {
                    session.AppForegrounded();
                }
            };
            session.Start();

            bridge.NotifyPause(true);
            time.Advance(TimeSpan.FromSeconds(4));

            Assert.AreEqual(GamePhase.Running, session.State.Phase);
            Assert.AreEqual(1, session.State.Level);
            Assert.AreEqual(0, session.State.ElapsedSecAtLevel);
        }

        [Test]
        public void PauseFalse_ResumesTick()
        {
            var time = new FakeTimeSource();
            using var session = new GameSession(new InMemoryGameProgressRepository(), time);
            var bridge = _host.AddComponent<LifecycleBridge>();
            bridge.ApplicationPaused += paused =>
            {
                if (paused)
                {
                    session.AppBackgrounded();
                }
                else
                {
                    session.AppForegrounded();
                }
            };
            session.Start();
            bridge.NotifyPause(true);

            bridge.NotifyPause(false);
            time.Advance(TimeSpan.FromSeconds(4));

            Assert.AreEqual(2, session.State.Level);
        }

        [Test]
        public void FocusLost_StopsTick_KeepsRunningPhase()
        {
            var time = new FakeTimeSource();
            using var session = new GameSession(new InMemoryGameProgressRepository(), time);
            var bridge = _host.AddComponent<LifecycleBridge>();
            bridge.ApplicationPaused += paused =>
            {
                if (paused)
                {
                    session.AppBackgrounded();
                }
                else
                {
                    session.AppForegrounded();
                }
            };
            session.Start();

            bridge.NotifyFocus(false);
            time.Advance(TimeSpan.FromSeconds(4));

            Assert.AreEqual(GamePhase.Running, session.State.Phase);
            Assert.AreEqual(1, session.State.Level);
            Assert.AreEqual(0, session.State.ElapsedSecAtLevel);
        }

        [Test]
        public void PauseThenFocusTrueWhilePaused_DoesNotResumeUntilPauseFalse()
        {
            var time = new FakeTimeSource();
            using var session = new GameSession(new InMemoryGameProgressRepository(), time);
            var bridge = _host.AddComponent<LifecycleBridge>();
            bridge.ApplicationPaused += paused =>
            {
                if (paused)
                {
                    session.AppBackgrounded();
                }
                else
                {
                    session.AppForegrounded();
                }
            };
            session.Start();
            bridge.NotifyPause(true);
            bridge.NotifyFocus(true);
            time.Advance(TimeSpan.FromSeconds(4));

            Assert.AreEqual(1, session.State.Level);

            bridge.NotifyPause(false);
            time.Advance(TimeSpan.FromSeconds(4));
            Assert.AreEqual(2, session.State.Level);
        }
    }
}
