using MindSilence.Presentation;
using NUnit.Framework;

namespace MindSilence.Tests.EditMode
{
    public sealed class MenuControllerTests
    {
        [Test]
        public void OpenHowToTrain_ShowsDialog()
        {
            var menu = new MenuController();

            menu.OpenHowToTrain();

            Assert.IsTrue(menu.ShowHowToTrain);
        }

        [Test]
        public void DismissHowToTrain_HidesDialog()
        {
            var menu = new MenuController();
            menu.OpenHowToTrain();

            menu.DismissHowToTrain();

            Assert.IsFalse(menu.ShowHowToTrain);
        }

        [Test]
        public void DismissHowToTrain_WhenHidden_IsNoOp()
        {
            var menu = new MenuController();
            var changed = 0;
            menu.Changed += () => changed++;

            menu.DismissHowToTrain();

            Assert.IsFalse(menu.ShowHowToTrain);
            Assert.AreEqual(0, changed);
        }

        [Test]
        public void OpenHowToTrain_WhenShown_IsNoOp()
        {
            var menu = new MenuController();
            menu.OpenHowToTrain();
            var changed = 0;
            menu.Changed += () => changed++;

            menu.OpenHowToTrain();

            Assert.IsTrue(menu.ShowHowToTrain);
            Assert.AreEqual(0, changed);
        }

        [Test]
        public void OpenTraining_EmitsNavigateToTraining()
        {
            var menu = new MenuController();
            var navigated = 0;
            menu.NavigateToTraining += () => navigated++;

            menu.OpenTraining();

            Assert.AreEqual(1, navigated);
            Assert.IsFalse(menu.ShowHowToTrain);
        }
    }
}
