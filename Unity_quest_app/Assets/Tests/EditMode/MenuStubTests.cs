using MindSilence.Presentation;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

namespace MindSilence.Tests.EditMode
{
    public sealed class MenuStubTests
    {
        GameObject _panel;

        [SetUp]
        public void SetUp()
        {
            SplashProcessGate.ResetForTests();
            _panel = new GameObject("WorldSpacePanel", typeof(RectTransform));
        }

        [TearDown]
        public void TearDown()
        {
            if (_panel != null)
            {
                Object.DestroyImmediate(_panel);
            }

            SplashProcessGate.ResetForTests();
        }

        [Test]
        public void BuildHierarchy_UsesAppStrings_NotHardcodedCopy()
        {
            var menu = CreateMenu();

            var training = menu.transform.Find(MenuStub.RowName + "/" + MenuStub.TrainingButtonName + "/Label")
                .GetComponent<Text>();
            var title = menu.transform.Find(MenuStub.OverlayName + "/" + MenuStub.CardName + "/" + MenuStub.TitleName)
                .GetComponent<Text>();
            var body = menu.transform.Find(MenuStub.OverlayName + "/" + MenuStub.CardName + "/" + MenuStub.BodyName)
                .GetComponent<Text>();
            var ok = menu.transform.Find(
                    MenuStub.OverlayName + "/" + MenuStub.CardName + "/" + MenuStub.FooterName + "/" +
                    MenuStub.OkButtonName + "/Label")
                .GetComponent<Text>();
            var howToTrainCd = menu.transform.Find(
                    MenuStub.RowName + "/" + MenuStub.InfoButtonName + "/ContentDescription")
                .GetComponent<Text>();

            Assert.AreEqual(AppStrings.MenuTraining, training.text);
            Assert.AreEqual(AppStrings.HowToTrainTitle, title.text);
            Assert.AreEqual(AppStrings.HowToTrainBody, body.text);
            Assert.AreEqual(AppStrings.Ok, ok.text);
            Assert.AreEqual(AppStrings.MenuHowToTrainCd, howToTrainCd.text);
        }

        [Test]
        public void InfoButton_IsSeparateFromTraining()
        {
            var menu = CreateMenu();
            var training = menu.transform.Find(MenuStub.RowName + "/" + MenuStub.TrainingButtonName);
            var info = menu.transform.Find(MenuStub.RowName + "/" + MenuStub.InfoButtonName);

            Assert.IsNotNull(training);
            Assert.IsNotNull(info);
            Assert.AreNotSame(training, info);
            Assert.AreEqual(training.parent, info.parent);
            Assert.AreEqual(MenuStub.RowName, training.parent.name);
        }

        [Test]
        public void Menu_HasNoSettingsOrHighscore()
        {
            var menu = CreateMenu();
            Assert.IsNull(FindRecursive(menu.transform, "Settings"));
            Assert.IsNull(FindRecursive(menu.transform, "Highscore"));
            Assert.IsNull(FindRecursive(menu.transform, "HighScores"));
        }

        [Test]
        public void InfoButton_OpensHowToTrain()
        {
            var menu = CreateMenu();
            Assert.IsFalse(menu.Controller.ShowHowToTrain);
            Assert.IsFalse(menu.transform.Find(MenuStub.OverlayName).gameObject.activeSelf);

            menu.transform.Find(MenuStub.RowName + "/" + MenuStub.InfoButtonName)
                .GetComponent<Button>()
                .onClick.Invoke();

            Assert.IsTrue(menu.Controller.ShowHowToTrain);
            Assert.IsTrue(menu.transform.Find(MenuStub.OverlayName).gameObject.activeSelf);
        }

        [Test]
        public void Ok_DismissesHowToTrain()
        {
            var menu = CreateMenu();
            menu.EmitOpenHowToTrain();

            menu.transform.Find(
                    MenuStub.OverlayName + "/" + MenuStub.CardName + "/" + MenuStub.FooterName + "/" +
                    MenuStub.OkButtonName)
                .GetComponent<Button>()
                .onClick.Invoke();

            Assert.IsFalse(menu.Controller.ShowHowToTrain);
            Assert.IsFalse(menu.transform.Find(MenuStub.OverlayName).gameObject.activeSelf);
        }

        [Test]
        public void Scrim_DismissesHowToTrain()
        {
            var menu = CreateMenu();
            menu.EmitOpenHowToTrain();

            menu.transform.Find(MenuStub.OverlayName + "/" + MenuStub.ScrimName)
                .GetComponent<Button>()
                .onClick.Invoke();

            Assert.IsFalse(menu.Controller.ShowHowToTrain);
        }

        [Test]
        public void TrainingButton_EmitsNavigateToTraining()
        {
            var menu = CreateMenu();
            var navigated = 0;
            menu.NavigateToTraining += () => navigated++;

            menu.transform.Find(MenuStub.RowName + "/" + MenuStub.TrainingButtonName)
                .GetComponent<Button>()
                .onClick.Invoke();

            Assert.AreEqual(1, navigated);
            Assert.IsFalse(menu.Controller.ShowHowToTrain);
        }

        [Test]
        public void HowToTrain_ShowsFullBody_WithoutOverlappingOk()
        {
            var menu = CreateMenu();
            menu.EmitOpenHowToTrain();

            var card = (RectTransform)menu.transform.Find(MenuStub.OverlayName + "/" + MenuStub.CardName);
            var body = (RectTransform)card.Find(MenuStub.BodyName);
            var ok = (RectTransform)card.Find(MenuStub.FooterName + "/" + MenuStub.OkButtonName);
            var bodyText = body.GetComponent<Text>();

            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(card);

            Assert.AreEqual(AppStrings.HowToTrainBody, bodyText.text);
            StringAssert.Contains("Sit comfortably", bodyText.text);
            StringAssert.Contains("as long as possible", bodyText.text);
            StringAssert.Contains("5 minutes a day", bodyText.text);
            Assert.Greater(body.rect.height, 0f);
            var bodyLocal = LocalRect(body, card);
            var okLocal = LocalRect(ok, card);
            Assert.IsFalse(bodyLocal.Overlaps(okLocal));
            Assert.GreaterOrEqual(bodyLocal.yMin, okLocal.yMax);
        }

        [Test]
        public void DoesNotReferenceAppNavigator()
        {
            Assert.IsNull(typeof(MenuStub).GetProperty("Navigator"));
            Assert.IsNull(typeof(MenuController).GetProperty("Navigator"));
        }

        MenuStub CreateMenu()
        {
            var panelRect = _panel.GetComponent<RectTransform>();
            panelRect.sizeDelta = new Vector2(AppHost.PanelWidth, AppHost.PanelHeight);
            if (_panel.GetComponent<Canvas>() == null)
            {
                _panel.AddComponent<Canvas>();
            }

            var menu = new GameObject(AppHost.MenuStubName, typeof(RectTransform), typeof(CanvasRenderer));
            menu.transform.SetParent(_panel.transform, false);
            var menuRect = menu.GetComponent<RectTransform>();
            menuRect.anchorMin = Vector2.zero;
            menuRect.anchorMax = Vector2.one;
            menuRect.offsetMin = Vector2.zero;
            menuRect.offsetMax = Vector2.zero;
            var stub = menu.AddComponent<MenuStub>();
            MenuStub.BuildHierarchy(menuRect);
            return stub;
        }

        static Rect LocalRect(RectTransform target, RectTransform relativeTo)
        {
            var corners = new Vector3[4];
            target.GetWorldCorners(corners);
            var min = relativeTo.InverseTransformPoint(corners[0]);
            var max = relativeTo.InverseTransformPoint(corners[2]);
            return Rect.MinMaxRect(
                Mathf.Min(min.x, max.x),
                Mathf.Min(min.y, max.y),
                Mathf.Max(min.x, max.x),
                Mathf.Max(min.y, max.y));
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
