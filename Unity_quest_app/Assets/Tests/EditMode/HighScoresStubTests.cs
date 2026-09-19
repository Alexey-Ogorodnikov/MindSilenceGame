using System;
using MindSilence.Data;
using MindSilence.Presentation;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

namespace MindSilence.Tests.EditMode
{
    public sealed class HighScoresStubTests
    {
        GameObject _panel;

        [SetUp]
        public void SetUp()
        {
            _panel = new GameObject("WorldSpacePanel", typeof(RectTransform));
        }

        [TearDown]
        public void TearDown()
        {
            if (_panel != null)
            {
                UnityEngine.Object.DestroyImmediate(_panel);
            }
        }

        [Test]
        public void BuildHierarchy_UsesAppStrings_NotHardcodedCopy()
        {
            var highScores = CreateHighScores();

            Assert.AreEqual(
                AppStrings.HighScoresTitle,
                highScores.transform.Find(AppHost.ContentRootName + "/" + HighScoresStub.TitleName).GetComponent<Text>().text);
            Assert.AreEqual(
                AppStrings.Back,
                highScores.transform.Find(AppHost.ContentRootName + "/" + HighScoresStub.BackButtonName + "/Label").GetComponent<Text>().text);
            Assert.AreEqual(
                AppStrings.HighScoresEmpty,
                highScores.transform.Find(AppHost.ContentRootName + "/" + HighScoresStub.EmptyName).GetComponent<Text>().text);
        }

        [Test]
        public void EmptyRepository_ShowsNoRecordsYet()
        {
            var highScores = CreateHighScores();
            highScores.Bind(new InMemoryGameProgressRepository());

            Assert.IsTrue(highScores.transform.Find(AppHost.ContentRootName + "/" + HighScoresStub.EmptyName).gameObject.activeSelf);
            Assert.IsFalse(highScores.transform.Find(AppHost.ContentRootName + "/" + HighScoresStub.ScrollName).gameObject.activeSelf);
            Assert.AreEqual(
                AppStrings.HighScoresEmpty,
                highScores.transform.Find(AppHost.ContentRootName + "/" + HighScoresStub.EmptyName).GetComponent<Text>().text);
        }

        [Test]
        public void Days_NewestFirst_EnglishDate_AndDailyCopy()
        {
            var older = new DateTime(2026, 9, 1);
            var newer = new DateTime(2026, 9, 5);
            var current = older;
            var repository = new InMemoryGameProgressRepository(() => current);
            repository.RecordSession(levelReached: 2, totalSeconds: 90);
            current = newer;
            repository.RecordSession(levelReached: 4, totalSeconds: 12);
            repository.RecordSession(levelReached: 3, totalSeconds: 8);

            var highScores = CreateHighScores();
            highScores.Bind(repository);

            Assert.IsFalse(highScores.transform.Find(AppHost.ContentRootName + "/" + HighScoresStub.EmptyName).gameObject.activeSelf);
            var content = highScores.transform.Find(
                AppHost.ContentRootName + "/" + HighScoresStub.ScrollName + "/" + HighScoresStub.ViewportName + "/" + HighScoresStub.ContentName);
            Assert.AreEqual(2, content.childCount);
            Assert.AreEqual(HighScoresStub.DayRowPrefix + "2026-09-05", content.GetChild(0).name);
            Assert.AreEqual(HighScoresStub.DayRowPrefix + "2026-09-01", content.GetChild(1).name);

            var newest = content.GetChild(0);
            Assert.AreEqual("5 September 2026", newest.Find(HighScoresStub.DayDateName).GetComponent<Text>().text);
            Assert.AreEqual(AppStrings.FormatDailyAttempts(2), newest.Find(HighScoresStub.DayAttemptsName).GetComponent<Text>().text);
            Assert.AreEqual(
                AppStrings.FormatDailyTotalTime(0, 20),
                newest.Find(HighScoresStub.DayTotalName).GetComponent<Text>().text);
            Assert.AreEqual(
                AppStrings.FormatDailyBestLevel(4),
                newest.Find(HighScoresStub.DayBestName).GetComponent<Text>().text);

            var oldest = content.GetChild(1);
            Assert.AreEqual("1 September 2026", oldest.Find(HighScoresStub.DayDateName).GetComponent<Text>().text);
            Assert.AreEqual(AppStrings.FormatDailyAttempts(1), oldest.Find(HighScoresStub.DayAttemptsName).GetComponent<Text>().text);
            Assert.AreEqual(
                AppStrings.FormatDailyTotalTime(1, 30),
                oldest.Find(HighScoresStub.DayTotalName).GetComponent<Text>().text);
        }

        [Test]
        public void Scroll_IsVerticalPokeTarget_WithoutLocomotion()
        {
            var highScores = CreateHighScores();
            var scroll = highScores.transform.Find(AppHost.ContentRootName + "/" + HighScoresStub.ScrollName).GetComponent<ScrollRect>();

            Assert.IsNotNull(scroll);
            Assert.IsTrue(scroll.vertical);
            Assert.IsFalse(scroll.horizontal);
            Assert.IsNotNull(scroll.viewport.GetComponent<RectMask2D>());
            Assert.IsTrue(scroll.viewport.GetComponent<Image>().raycastTarget);
            Assert.IsNotNull(scroll.verticalScrollbar);
            Assert.AreEqual(HighScoresStub.ScrollbarWidth, ((RectTransform)scroll.verticalScrollbar.transform).sizeDelta.x);
        }

        [Test]
        public void Back_EmitsNavigateBack()
        {
            var highScores = CreateHighScores();
            var left = 0;
            highScores.NavigateBack += () => left++;

            highScores.transform.Find(AppHost.ContentRootName + "/" + HighScoresStub.BackButtonName).GetComponent<Button>().onClick.Invoke();

            Assert.AreEqual(1, left);
        }

        [Test]
        public void DoesNotReferenceAppNavigator()
        {
            Assert.IsNull(typeof(HighScoresStub).GetProperty("Navigator"));
        }

        HighScoresStub CreateHighScores()
        {
            var go = new GameObject(AppHost.HighScoresStubName, typeof(RectTransform), typeof(CanvasRenderer));
            go.transform.SetParent(_panel.transform, false);
            var stub = go.AddComponent<HighScoresStub>();
            HighScoresStub.BuildHierarchy(go.GetComponent<RectTransform>());
            return stub;
        }
    }
}
