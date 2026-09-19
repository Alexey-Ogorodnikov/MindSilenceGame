using MindSilence.Presentation;
using NUnit.Framework;

namespace MindSilence.Tests.EditMode
{
    public sealed class AppNavigatorTests
    {
        [Test]
        public void StartsOnMenu()
        {
            var navigator = new AppNavigator();

            Assert.IsFalse(navigator.InTraining);
            Assert.IsFalse(navigator.ShowHighScores);
            Assert.IsTrue(navigator.ShowMenu);
            Assert.IsFalse(navigator.ShowTraining);
            Assert.AreEqual(new AppUiState(false, false), navigator.State);
        }

        [Test]
        public void OpenTraining_EntersTraining()
        {
            var navigator = new AppNavigator();

            navigator.OpenTraining();

            Assert.IsTrue(navigator.InTraining);
            Assert.IsFalse(navigator.ShowHighScores);
            Assert.IsTrue(navigator.ShowTraining);
            Assert.IsFalse(navigator.ShowMenu);
        }

        [Test]
        public void LeaveTraining_ReturnsToMenu()
        {
            var navigator = new AppNavigator();
            navigator.OpenTraining();

            navigator.LeaveTraining();

            Assert.IsFalse(navigator.InTraining);
            Assert.IsFalse(navigator.ShowHighScores);
            Assert.IsTrue(navigator.ShowMenu);
        }

        [Test]
        public void OpenHighScores_DoesNotClearInTraining()
        {
            var navigator = new AppNavigator();
            navigator.OpenTraining();

            navigator.OpenHighScores();

            Assert.IsTrue(navigator.InTraining);
            Assert.IsTrue(navigator.ShowHighScores);
            Assert.IsFalse(navigator.ShowTraining);
            Assert.IsFalse(navigator.ShowMenu);
            Assert.AreEqual(new AppUiState(true, true), navigator.State);
        }

        [Test]
        public void LeaveHighScores_ReturnsToTrainingFlag()
        {
            var navigator = new AppNavigator();
            navigator.OpenTraining();
            navigator.OpenHighScores();

            navigator.LeaveHighScores();

            Assert.IsTrue(navigator.InTraining);
            Assert.IsFalse(navigator.ShowHighScores);
            Assert.IsTrue(navigator.ShowTraining);
            Assert.IsFalse(navigator.ShowMenu);
        }

        [Test]
        public void NewInstance_IsMenu_LikeColdStartAfterKill()
        {
            var navigator = new AppNavigator();
            navigator.OpenTraining();
            navigator.OpenHighScores();

            var afterKill = new AppNavigator();

            Assert.IsFalse(afterKill.InTraining);
            Assert.IsFalse(afterKill.ShowHighScores);
            Assert.IsTrue(afterKill.ShowMenu);
        }
    }
}
