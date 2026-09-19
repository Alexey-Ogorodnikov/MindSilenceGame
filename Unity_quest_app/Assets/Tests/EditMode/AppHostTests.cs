using System;
using MindSilence.Data;
using MindSilence.Domain;
using MindSilence.Presentation;
using MindSilence.XR;
using NUnit.Framework;
using UnityEngine;

namespace MindSilence.Tests.EditMode
{
    public sealed class AppHostTests
    {
        GameObject _panel;

        [SetUp]
        public void SetUp()
        {
            SplashProcessGate.ResetForTests();
            AppHost.TestRepository = new InMemoryGameProgressRepository();
            AppHost.TestTime = new FakeTimeSource();
            _panel = new GameObject("WorldSpacePanel", typeof(RectTransform));
        }

        [TearDown]
        public void TearDown()
        {
            if (_panel != null)
            {
                UnityEngine.Object.DestroyImmediate(_panel);
            }

            AppHost.TestRepository = null;
            AppHost.TestTime = null;
            KeepAwakeBridge.SetEnabled(false);
            SplashProcessGate.ResetForTests();
        }

        [Test]
        public void AfterSplash_ShowsMenuStub()
        {
            var host = _panel.AddComponent<AppHost>();

            Assert.IsTrue(host.Menu.gameObject.activeSelf);
            Assert.IsFalse(host.Training.gameObject.activeSelf);
            Assert.IsFalse(host.HighScores.gameObject.activeSelf);
            Assert.IsTrue(host.Navigator.ShowMenu);
        }

        [Test]
        public void ColdSplash_HidesAllStubs()
        {
            var splashHost = _panel.AddComponent<SplashHost>();
            var host = _panel.GetComponent<AppHost>();

            Assert.IsNotNull(host);
            Assert.IsTrue(splashHost.Controller.ShowBrandedSplash);
            Assert.IsFalse(host.Menu.gameObject.activeSelf);
            Assert.IsFalse(host.Training.gameObject.activeSelf);
            Assert.IsFalse(host.HighScores.gameObject.activeSelf);
        }

        [Test]
        public void RepeatEntry_SkipsSplash_ShowsMenuStub()
        {
            SplashProcessGate.ConsumeColdStart();
            var splashHost = _panel.AddComponent<SplashHost>();
            var host = _panel.GetComponent<AppHost>();

            Assert.IsFalse(splashHost.Controller.ShowBrandedSplash);
            Assert.IsTrue(host.Menu.gameObject.activeSelf);
            Assert.IsFalse(host.Training.gameObject.activeSelf);
            Assert.IsFalse(host.HighScores.gameObject.activeSelf);
        }

        [Test]
        public void MenuTrainingButton_OpensTrainingStub()
        {
            var host = _panel.AddComponent<AppHost>();

            host.Menu.transform.Find(MenuStub.RowName + "/" + MenuStub.TrainingButtonName)
                .GetComponent<UnityEngine.UI.Button>()
                .onClick.Invoke();

            Assert.IsTrue(host.Navigator.InTraining);
            Assert.IsFalse(host.Menu.gameObject.activeSelf);
            Assert.IsTrue(host.Training.gameObject.activeSelf);
        }

        [Test]
        public void MenuEffect_OpensTrainingStub()
        {
            var host = _panel.AddComponent<AppHost>();

            host.Menu.EmitNavigateToTraining();

            Assert.IsTrue(host.Navigator.InTraining);
            Assert.IsFalse(host.Menu.gameObject.activeSelf);
            Assert.IsTrue(host.Training.gameObject.activeSelf);
            Assert.IsFalse(host.HighScores.gameObject.activeSelf);
        }

        [Test]
        public void TrainingEffect_OpenHighScores_KeepsInTraining()
        {
            var host = _panel.AddComponent<AppHost>();
            host.Menu.EmitNavigateToTraining();

            host.Training.EmitNavigateToHighScores();

            Assert.IsTrue(host.Navigator.InTraining);
            Assert.IsTrue(host.Navigator.ShowHighScores);
            Assert.IsFalse(host.Menu.gameObject.activeSelf);
            Assert.IsFalse(host.Training.gameObject.activeSelf);
            Assert.IsTrue(host.HighScores.gameObject.activeSelf);
        }

        [Test]
        public void HighScoresEffect_Leave_ReturnsToTrainingStub()
        {
            var host = _panel.AddComponent<AppHost>();
            host.Menu.EmitNavigateToTraining();
            host.Training.EmitNavigateToHighScores();

            host.HighScores.EmitNavigateBack();

            Assert.IsTrue(host.Navigator.InTraining);
            Assert.IsFalse(host.Navigator.ShowHighScores);
            Assert.IsFalse(host.Menu.gameObject.activeSelf);
            Assert.IsTrue(host.Training.gameObject.activeSelf);
            Assert.IsFalse(host.HighScores.gameObject.activeSelf);
        }

        [Test]
        public void TrainingEffect_Leave_ReturnsToMenuStub()
        {
            var host = _panel.AddComponent<AppHost>();
            host.Menu.EmitNavigateToTraining();

            host.Training.EmitNavigateBackToMenu();

            Assert.IsFalse(host.Navigator.InTraining);
            Assert.IsTrue(host.Menu.gameObject.activeSelf);
            Assert.IsFalse(host.Training.gameObject.activeSelf);
        }

        [Test]
        public void Stubs_DoNotReferenceAppNavigator()
        {
            Assert.IsNull(typeof(MenuStub).GetProperty("Navigator"));
            Assert.IsNull(typeof(TrainingStub).GetProperty("Navigator"));
            Assert.IsNull(typeof(HighScoresStub).GetProperty("Navigator"));
        }

        [Test]
        public void OpenTraining_CreatesIdleSession_SameRepository()
        {
            var host = _panel.AddComponent<AppHost>();
            host.Menu.EmitNavigateToTraining();

            Assert.IsNotNull(host.Session);
            Assert.AreEqual(GamePhase.Idle, host.Session.State.Phase);
            Assert.AreSame(host.Progress, AppHost.TestRepository);
            Assert.AreSame(host.TimeSource, AppHost.TestTime);
        }

        [Test]
        public void StartThenFourSeconds_ReachesLevelTwo()
        {
            var host = _panel.AddComponent<AppHost>();
            host.Menu.EmitNavigateToTraining();

            host.Training.transform.Find(AppHost.ContentRootName + "/" + TrainingStub.ButtonRowName + "/" + TrainingStub.StartButtonName)
                .GetComponent<UnityEngine.UI.Button>()
                .onClick.Invoke();
            ((FakeTimeSource)host.TimeSource).Advance(TimeSpan.FromSeconds(4));

            Assert.AreEqual(2, host.Session.State.Level);
            Assert.AreEqual(
                "2",
                host.Training.transform.Find(
                        AppHost.ContentRootName + "/" + TrainingStub.FocusName + "/" + TrainingStub.RingSlotName + "/" + TrainingStub.LevelGlyphName)
                    .GetComponent<UnityEngine.UI.Text>().text);
        }

        [Test]
        public void Thought_ShowsSessionComplete_OkStaysOnTraining()
        {
            var host = _panel.AddComponent<AppHost>();
            host.Menu.EmitNavigateToTraining();
            host.Training.transform.Find(AppHost.ContentRootName + "/" + TrainingStub.ButtonRowName + "/" + TrainingStub.StartButtonName)
                .GetComponent<UnityEngine.UI.Button>()
                .onClick.Invoke();
            ((FakeTimeSource)host.TimeSource).Advance(TimeSpan.FromSeconds(4));

            host.Training.transform.Find(AppHost.ContentRootName + "/" + TrainingStub.ButtonRowName + "/" + TrainingStub.ThoughtButtonName)
                .GetComponent<UnityEngine.UI.Button>()
                .onClick.Invoke();

            Assert.AreEqual(GamePhase.Idle, host.Session.State.Phase);
            Assert.AreEqual(1, host.Progress.GetDailyStats().Count);
            var overlay = host.Training.transform.Find(TrainingStub.SummaryOverlayName);
            Assert.IsTrue(overlay.gameObject.activeSelf);

            overlay.Find(TrainingStub.SummaryCardName + "/" + TrainingStub.SummaryOkButtonName)
                .GetComponent<UnityEngine.UI.Button>()
                .onClick.Invoke();

            Assert.IsFalse(overlay.gameObject.activeSelf);
            Assert.IsTrue(host.Training.gameObject.activeSelf);
            Assert.IsTrue(host.Navigator.InTraining);
            Assert.IsFalse(host.Navigator.ShowHighScores);
            Assert.IsTrue(
                host.Training.transform.Find(AppHost.ContentRootName + "/" + TrainingStub.ButtonRowName + "/" + TrainingStub.StartButtonName)
                    .GetComponent<UnityEngine.UI.Button>()
                    .interactable);

            host.Training.transform.Find(AppHost.ContentRootName + "/" + TrainingStub.BackButtonName)
                .GetComponent<UnityEngine.UI.Button>()
                .onClick.Invoke();

            Assert.IsTrue(host.Navigator.ShowMenu);
            Assert.IsNull(host.Session);
        }

        [Test]
        public void Thought_Highscore_Back_CreatesNewIdleTraining()
        {
            AppHost.TestRepository = new InMemoryGameProgressRepository(() => new DateTime(2026, 9, 5));
            var host = _panel.AddComponent<AppHost>();
            host.Menu.EmitNavigateToTraining();
            var firstSession = host.Session;
            host.Training.transform.Find(AppHost.ContentRootName + "/" + TrainingStub.ButtonRowName + "/" + TrainingStub.StartButtonName)
                .GetComponent<UnityEngine.UI.Button>()
                .onClick.Invoke();
            ((FakeTimeSource)host.TimeSource).Advance(TimeSpan.FromSeconds(4));
            host.Training.transform.Find(AppHost.ContentRootName + "/" + TrainingStub.ButtonRowName + "/" + TrainingStub.ThoughtButtonName)
                .GetComponent<UnityEngine.UI.Button>()
                .onClick.Invoke();

            host.Training.transform.Find(
                    TrainingStub.SummaryOverlayName + "/" + TrainingStub.SummaryCardName + "/"
                    + TrainingStub.SummaryHighscoreButtonName)
                .GetComponent<UnityEngine.UI.Button>()
                .onClick.Invoke();

            Assert.IsTrue(host.Navigator.InTraining);
            Assert.IsTrue(host.Navigator.ShowHighScores);
            Assert.IsFalse(host.Training.gameObject.activeSelf);
            Assert.IsTrue(host.HighScores.gameObject.activeSelf);
            Assert.IsNull(host.Session);
            Assert.AreSame(host.Progress, AppHost.TestRepository);
            var day = host.HighScores.transform.Find(
                AppHost.ContentRootName + "/" + HighScoresStub.ScrollName + "/" + HighScoresStub.ViewportName + "/" + HighScoresStub.ContentName
                + "/" + HighScoresStub.DayRowPrefix + "2026-09-05");
            Assert.IsNotNull(day);
            Assert.AreEqual(
                "5 September 2026",
                day.Find(HighScoresStub.DayDateName).GetComponent<UnityEngine.UI.Text>().text);

            host.HighScores.transform.Find(AppHost.ContentRootName + "/" + HighScoresStub.BackButtonName)
                .GetComponent<UnityEngine.UI.Button>()
                .onClick.Invoke();

            Assert.IsTrue(host.Navigator.InTraining);
            Assert.IsFalse(host.Navigator.ShowHighScores);
            Assert.IsTrue(host.Training.gameObject.activeSelf);
            Assert.IsFalse(host.HighScores.gameObject.activeSelf);
            Assert.IsFalse(host.Menu.gameObject.activeSelf);
            Assert.IsNotNull(host.Session);
            Assert.AreNotSame(firstSession, host.Session);
            Assert.AreEqual(GamePhase.Idle, host.Session.State.Phase);
            Assert.IsNull(host.Session.State.SessionSummary);
            Assert.IsFalse(host.Training.transform.Find(TrainingStub.SummaryOverlayName).gameObject.activeSelf);
            Assert.IsTrue(
                host.Training.transform.Find(AppHost.ContentRootName + "/" + TrainingStub.ButtonRowName + "/" + TrainingStub.StartButtonName)
                    .GetComponent<UnityEngine.UI.Button>()
                    .interactable);
        }

        [Test]
        public void Menu_HasNoHighscoreButton()
        {
            var host = _panel.AddComponent<AppHost>();
            Assert.IsNull(FindRecursive(host.Menu.transform, "Highscore"));
            Assert.IsNull(FindRecursive(host.Menu.transform, TrainingStub.SummaryHighscoreButtonName));
        }

        [Test]
        public void HeadsetPause_StopsTickWhileRunning()
        {
            var host = _panel.AddComponent<AppHost>();
            host.Menu.EmitNavigateToTraining();
            host.Training.transform.Find(AppHost.ContentRootName + "/" + TrainingStub.ButtonRowName + "/" + TrainingStub.StartButtonName)
                .GetComponent<UnityEngine.UI.Button>()
                .onClick.Invoke();

            host.GetComponent<LifecycleBridge>().NotifyPause(true);
            ((FakeTimeSource)host.TimeSource).Advance(TimeSpan.FromSeconds(4));

            Assert.AreEqual(GamePhase.Running, host.Session.State.Phase);
            Assert.AreEqual(1, host.Session.State.Level);
            Assert.AreEqual(0, host.Session.State.ElapsedSecAtLevel);
            Assert.AreEqual(SleepTimeout.SystemSetting, Screen.sleepTimeout);
        }

        [Test]
        public void HeadsetFocusLost_StopsTickWhileRunning()
        {
            var host = _panel.AddComponent<AppHost>();
            host.Menu.EmitNavigateToTraining();
            host.Training.transform.Find(AppHost.ContentRootName + "/" + TrainingStub.ButtonRowName + "/" + TrainingStub.StartButtonName)
                .GetComponent<UnityEngine.UI.Button>()
                .onClick.Invoke();
            Assert.AreEqual(SleepTimeout.NeverSleep, Screen.sleepTimeout);

            host.GetComponent<LifecycleBridge>().NotifyFocus(false);
            ((FakeTimeSource)host.TimeSource).Advance(TimeSpan.FromSeconds(4));

            Assert.AreEqual(GamePhase.Running, host.Session.State.Phase);
            Assert.AreEqual(1, host.Session.State.Level);
            Assert.AreEqual(0, host.Session.State.ElapsedSecAtLevel);
            Assert.AreEqual(SleepTimeout.SystemSetting, Screen.sleepTimeout);
        }

        [Test]
        public void Thought_IsTrainingButtonNotAirGesture()
        {
            var host = _panel.AddComponent<AppHost>();
            host.Menu.EmitNavigateToTraining();
            host.Training.transform.Find(AppHost.ContentRootName + "/" + TrainingStub.ButtonRowName + "/" + TrainingStub.StartButtonName)
                .GetComponent<UnityEngine.UI.Button>()
                .onClick.Invoke();

            Assert.IsNull(host.GetComponent<ControllerTriggerSelect>());
            host.Training.transform.Find(AppHost.ContentRootName + "/" + TrainingStub.ButtonRowName + "/" + TrainingStub.ThoughtButtonName)
                .GetComponent<UnityEngine.UI.Button>()
                .onClick.Invoke();

            Assert.AreEqual(GamePhase.Idle, host.Session.State.Phase);
            Assert.IsNotNull(host.Session.State.SessionSummary);
        }

        static Transform FindRecursive(Transform root, string name)
        {
            if (root.name == name)
            {
                return root;
            }

            for (var i = 0; i < root.childCount; i++)
            {
                var found = FindRecursive(root.GetChild(i), name);
                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }
    }
}
