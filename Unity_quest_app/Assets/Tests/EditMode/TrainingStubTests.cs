using System;
using MindSilence.Data;
using MindSilence.Domain;
using MindSilence.Presentation;
using MindSilence.XR;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

namespace MindSilence.Tests.EditMode
{
    public sealed class TrainingStubTests
    {
        GameObject _panel;
        InMemoryGameProgressRepository _repository;
        FakeTimeSource _time;
        GameSession _session;

        [SetUp]
        public void SetUp()
        {
            _panel = new GameObject("WorldSpacePanel", typeof(RectTransform));
            _repository = new InMemoryGameProgressRepository();
            _time = new FakeTimeSource();
            _session = new GameSession(_repository, _time);
        }

        [TearDown]
        public void TearDown()
        {
            _session?.Dispose();
            _session = null;
            if (_panel != null)
            {
                UnityEngine.Object.DestroyImmediate(_panel);
            }

            KeepAwakeBridge.SetEnabled(false);
        }

        [Test]
        public void BuildHierarchy_UsesAppStrings_NotHardcodedCopy()
        {
            var training = CreateTraining();

            Assert.AreEqual(
                AppStrings.LevelLabel,
                training.transform.Find(AppHost.ContentRootName + "/" + TrainingStub.FocusName + "/" + TrainingStub.LevelLabelName).GetComponent<Text>().text);
            Assert.AreEqual(
                AppStrings.Start,
                training.transform.Find(AppHost.ContentRootName + "/" + TrainingStub.ButtonRowName + "/" + TrainingStub.StartButtonName + "/Label")
                    .GetComponent<Text>().text);
            Assert.AreEqual(
                AppStrings.Thought,
                training.transform.Find(AppHost.ContentRootName + "/" + TrainingStub.ButtonRowName + "/" + TrainingStub.ThoughtButtonName + "/Label")
                    .GetComponent<Text>().text);
            Assert.AreEqual(
                AppStrings.Back,
                training.transform.Find(AppHost.ContentRootName + "/" + TrainingStub.BackButtonName + "/Label").GetComponent<Text>().text);
            Assert.AreEqual(
                AppStrings.StartContentDescription,
                training.transform.Find(
                        AppHost.ContentRootName + "/" + TrainingStub.ButtonRowName + "/" + TrainingStub.StartButtonName + "/ContentDescription")
                    .GetComponent<Text>().text);
            Assert.AreEqual(
                AppStrings.ThoughtContentDescription,
                training.transform.Find(
                        AppHost.ContentRootName + "/" + TrainingStub.ButtonRowName + "/" + TrainingStub.ThoughtButtonName + "/ContentDescription")
                    .GetComponent<Text>().text);
            Assert.AreEqual(
                AppStrings.SessionSummaryTitle,
                training.transform.Find(
                        TrainingStub.SummaryOverlayName + "/" + TrainingStub.SummaryCardName + "/"
                        + TrainingStub.SummaryTitleName)
                    .GetComponent<Text>().text);
            Assert.AreEqual(
                AppStrings.Ok,
                training.transform.Find(
                        TrainingStub.SummaryOverlayName + "/" + TrainingStub.SummaryCardName + "/"
                        + TrainingStub.SummaryOkButtonName + "/Label")
                    .GetComponent<Text>().text);
            Assert.AreEqual(
                AppStrings.Highscore,
                training.transform.Find(
                        TrainingStub.SummaryOverlayName + "/" + TrainingStub.SummaryCardName + "/"
                        + TrainingStub.SummaryHighscoreButtonName + "/Label")
                    .GetComponent<Text>().text);
        }

        [Test]
        public void Idle_ShowsPlaceholder_StartEnabled_ThoughtDisabled()
        {
            var training = CreateTraining();
            training.Bind(_session);

            Assert.AreEqual(AppStrings.LevelIdle, Glyph(training).text);
            Assert.IsTrue(StartButton(training).interactable);
            Assert.IsFalse(ThoughtButton(training).interactable);
            Assert.AreEqual(0f, ProgressGroup(training).alpha);
            Assert.IsTrue(ProgressGroup(training).gameObject.activeSelf);
        }

        [Test]
        public void Start_ShowsLevelOneAndEnablesThought()
        {
            var training = CreateTraining();
            training.Bind(_session);

            StartButton(training).onClick.Invoke();

            Assert.AreEqual(GamePhase.Running, _session.State.Phase);
            Assert.AreEqual("1", Glyph(training).text);
            Assert.IsFalse(StartButton(training).interactable);
            Assert.IsTrue(ThoughtButton(training).interactable);
            Assert.AreEqual(1f, ProgressGroup(training).alpha);
            Assert.AreEqual(AppStrings.FormatLevelProgress(0, 4), ProgressLabel(training).text);
        }

        [Test]
        public void FourSeconds_AdvancesToLevelTwo()
        {
            var training = CreateTraining();
            training.Bind(_session);
            StartButton(training).onClick.Invoke();

            _time.Advance(TimeSpan.FromSeconds(4));

            Assert.AreEqual(2, _session.State.Level);
            Assert.AreEqual("2", Glyph(training).text);
            Assert.AreEqual(AppStrings.FormatLevelProgress(0, 8), ProgressLabel(training).text);
        }

        [Test]
        public void Thought_ReturnsToIdle_RecordsDay_ShowsSessionComplete()
        {
            var training = CreateTraining();
            training.Bind(_session);
            StartButton(training).onClick.Invoke();
            _time.Advance(TimeSpan.FromSeconds(4));

            ThoughtButton(training).onClick.Invoke();

            Assert.AreEqual(GamePhase.Idle, _session.State.Phase);
            Assert.AreEqual(AppStrings.LevelIdle, Glyph(training).text);
            Assert.IsFalse(StartButton(training).interactable);
            Assert.IsFalse(ThoughtButton(training).interactable);
            Assert.AreEqual(0f, ProgressGroup(training).alpha);
            Assert.AreEqual(1, _repository.GetDailyStats().Count);
            Assert.AreEqual(2, _repository.GetDailyStats()[0].BestLevel);
            var overlay = training.transform.Find(TrainingStub.SummaryOverlayName);
            Assert.IsTrue(overlay.gameObject.activeSelf);
            Assert.AreEqual(
                AppStrings.FormatSessionLevelReached(2),
                overlay.Find(TrainingStub.SummaryCardName + "/" + TrainingStub.SummaryLevelName).GetComponent<Text>().text);
            Assert.AreEqual(
                AppStrings.FormatSessionBestToday(2),
                overlay.Find(TrainingStub.SummaryCardName + "/" + TrainingStub.SummaryBestName).GetComponent<Text>().text);
            Assert.AreEqual(
                AppStrings.FormatSessionAttemptDuration(0, 4),
                overlay.Find(TrainingStub.SummaryCardName + "/" + TrainingStub.SummaryDurationName)
                    .GetComponent<Text>().text);
        }

        [Test]
        public void SessionCompleteOk_DismissesSummary_StartEnabled()
        {
            var training = CreateTraining();
            training.Bind(_session);
            StartButton(training).onClick.Invoke();
            ThoughtButton(training).onClick.Invoke();

            training.transform.Find(
                    TrainingStub.SummaryOverlayName + "/" + TrainingStub.SummaryCardName + "/"
                    + TrainingStub.SummaryOkButtonName)
                .GetComponent<Button>()
                .onClick.Invoke();

            Assert.IsNull(_session.State.SessionSummary);
            Assert.IsFalse(training.transform.Find(TrainingStub.SummaryOverlayName).gameObject.activeSelf);
            Assert.AreEqual(GamePhase.Idle, _session.State.Phase);
            Assert.IsTrue(StartButton(training).interactable);
        }

        [Test]
        public void SessionCompleteScrim_DismissesSummary()
        {
            var training = CreateTraining();
            training.Bind(_session);
            StartButton(training).onClick.Invoke();
            ThoughtButton(training).onClick.Invoke();

            training.transform.Find(TrainingStub.SummaryOverlayName + "/" + TrainingStub.SummaryScrimName)
                .GetComponent<Button>()
                .onClick.Invoke();

            Assert.IsNull(_session.State.SessionSummary);
            Assert.IsFalse(training.transform.Find(TrainingStub.SummaryOverlayName).gameObject.activeSelf);
        }

        [Test]
        public void SessionCompleteHighscore_ClearsSummaryAndNavigates()
        {
            var training = CreateTraining();
            training.Bind(_session);
            var navigated = 0;
            training.NavigateToHighScores += () => navigated++;
            StartButton(training).onClick.Invoke();
            ThoughtButton(training).onClick.Invoke();

            training.transform.Find(
                    TrainingStub.SummaryOverlayName + "/" + TrainingStub.SummaryCardName + "/"
                    + TrainingStub.SummaryHighscoreButtonName)
                .GetComponent<Button>()
                .onClick.Invoke();

            Assert.AreEqual(1, navigated);
            Assert.IsNull(_session.State.SessionSummary);
        }

        [Test]
        public void Back_EmitsLeaveTraining()
        {
            var training = CreateTraining();
            training.Bind(_session);
            var left = 0;
            training.NavigateBackToMenu += () => left++;

            training.transform.Find(AppHost.ContentRootName + "/" + TrainingStub.BackButtonName).GetComponent<Button>().onClick.Invoke();

            Assert.AreEqual(1, left);
        }

        [Test]
        public void LevelGlyph_SitsOnNeonCentroid()
        {
            var training = CreateTraining();
            var glyph = training.transform.Find(
                    AppHost.ContentRootName + "/" + TrainingStub.FocusName + "/" + TrainingStub.RingSlotName + "/" + TrainingStub.LevelGlyphName)
                as RectTransform;

            Assert.AreEqual(TrainingLayout.LevelGlyphAnchoredPosition(), glyph.anchoredPosition);
            Assert.AreEqual(625.35f / 1254f, TrainingLayout.RingCenterXFraction, 0.00001f);
            Assert.AreEqual(614.37f / 1254f, TrainingLayout.RingCenterYFraction, 0.00001f);
        }

        [Test]
        public void RingSlot_IsSquareAtDoubleHeight()
        {
            var training = CreateTraining();
            var focus = training.transform.Find(AppHost.ContentRootName + "/" + TrainingStub.FocusName) as RectTransform;
            var slot = training.transform.Find(
                    AppHost.ContentRootName + "/" + TrainingStub.FocusName + "/" + TrainingStub.RingSlotName)
                as RectTransform;
            var layout = slot.GetComponent<LayoutElement>();
            var column = focus.GetComponent<VerticalLayoutGroup>();

            Assert.AreEqual(1280f, TrainingLayout.RingSize);
            Assert.AreEqual(new Vector2(TrainingLayout.RingSize, TrainingLayout.RingSize), slot.sizeDelta);
            Assert.AreEqual(TrainingLayout.RingSize, layout.preferredWidth);
            Assert.AreEqual(TrainingLayout.RingSize, layout.preferredHeight);
            Assert.AreEqual(0f, layout.flexibleWidth);
            Assert.IsFalse(column.childForceExpandWidth);
            Assert.AreEqual(new Vector2(TrainingLayout.FocusWidth, TrainingLayout.FocusHeight), focus.sizeDelta);
        }

        [Test]
        public void RingImage_UsesProvidedSprite()
        {
            var texture = new Texture2D(2, 2);
            var sprite = Sprite.Create(texture, new Rect(0f, 0f, 2f, 2f), new Vector2(0.5f, 0.5f));
            var training = CreateTraining(sprite);
            var image = training.transform.Find(
                    AppHost.ContentRootName + "/" + TrainingStub.FocusName + "/" + TrainingStub.RingSlotName + "/"
                    + TrainingStub.RingImageName)
                .GetComponent<Image>();

            Assert.AreSame(sprite, image.sprite);
            Assert.AreEqual(Color.white, image.color);
            Assert.IsTrue(image.preserveAspect);
            Assert.IsFalse(image.raycastTarget);
        }

        [Test]
        public void IdleProgress_KeepsRingFromJumping()
        {
            var training = CreateTraining();
            training.Bind(_session);
            var ring = training.transform.Find(AppHost.ContentRootName + "/" + TrainingStub.FocusName + "/" + TrainingStub.RingSlotName) as RectTransform;
            var before = ring.anchoredPosition;
            var progress = ProgressGroup(training);
            Assert.IsTrue(progress.gameObject.activeSelf);
            Assert.AreEqual(0f, progress.alpha);

            StartButton(training).onClick.Invoke();

            Assert.AreEqual(before, ring.anchoredPosition);
            Assert.IsTrue(progress.gameObject.activeSelf);
        }

        [Test]
        public void DoesNotReferenceAppNavigator()
        {
            Assert.IsNull(typeof(TrainingStub).GetProperty("Navigator"));
        }

        TrainingStub CreateTraining(Sprite ring = null)
        {
            var go = new GameObject(AppHost.TrainingStubName, typeof(RectTransform), typeof(CanvasRenderer));
            go.transform.SetParent(_panel.transform, false);
            var stub = go.AddComponent<TrainingStub>();
            TrainingStub.BuildHierarchy(go.GetComponent<RectTransform>(), ring);
            return stub;
        }

        static Text Glyph(TrainingStub training) =>
            training.transform.Find(
                    AppHost.ContentRootName + "/" + TrainingStub.FocusName + "/" + TrainingStub.RingSlotName + "/" + TrainingStub.LevelGlyphName)
                .GetComponent<Text>();

        static Text ProgressLabel(TrainingStub training) =>
            training.transform.Find(
                    AppHost.ContentRootName + "/" + TrainingStub.FocusName + "/" + TrainingStub.ProgressName + "/" + TrainingStub.ProgressLabelName)
                .GetComponent<Text>();

        static CanvasGroup ProgressGroup(TrainingStub training) =>
            training.transform.Find(AppHost.ContentRootName + "/" + TrainingStub.FocusName + "/" + TrainingStub.ProgressName).GetComponent<CanvasGroup>();

        static Button StartButton(TrainingStub training) =>
            training.transform.Find(AppHost.ContentRootName + "/" + TrainingStub.ButtonRowName + "/" + TrainingStub.StartButtonName).GetComponent<Button>();

        static Button ThoughtButton(TrainingStub training) =>
            training.transform.Find(AppHost.ContentRootName + "/" + TrainingStub.ButtonRowName + "/" + TrainingStub.ThoughtButtonName)
                .GetComponent<Button>();
    }
}
