using MindSilence.XR;
using NUnit.Framework;
using UnityEngine;

namespace MindSilence.Tests.EditMode
{
    public sealed class QuestPointerVisualTests
    {
        [Test]
        public void Hover_IsQuestWhite()
        {
            Assert.AreEqual(Color.white, QuestPointerVisual.Hover);
            var hover = QuestPointerVisual.CreateSolid(QuestPointerVisual.Hover);
            Assert.AreEqual(Color.white, hover.Evaluate(0f));
            Assert.AreEqual(Color.white, hover.Evaluate(1f));
        }

        [Test]
        public void Selecting_IsQuestDarkBlue()
        {
            Assert.AreEqual((Color)new Color32(0x00, 0x1E, 0x78, 0xFF), QuestPointerVisual.Selecting);
        }

        [Test]
        public void OverlayWidth_MatchesQuestSpec()
        {
            Assert.AreEqual(0.001f, QuestPointerVisual.OverlayWidthMeters);
        }
    }
}
