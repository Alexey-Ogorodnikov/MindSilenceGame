using MindSilence.XR;
using NUnit.Framework;

namespace MindSilence.Tests.EditMode
{
    public sealed class HandPinchSelectTests
    {
        [Test]
        public void Pinch_Selects()
        {
            Assert.IsTrue(HandPinchSelect.ShouldSelect(
                indexPinching: true,
                pinchStrength: 0f,
                triggerButton: false,
                triggerAnalog: 0f,
                menuButton: false));
            Assert.IsTrue(HandPinchSelect.ShouldSelect(
                indexPinching: false,
                pinchStrength: 0.8f,
                triggerButton: false,
                triggerAnalog: 0f,
                menuButton: false));
        }

        [Test]
        public void MenuButton_DoesNotSelect()
        {
            Assert.IsFalse(HandPinchSelect.ShouldSelect(
                indexPinching: true,
                pinchStrength: 1f,
                triggerButton: true,
                triggerAnalog: 1f,
                menuButton: true));
        }

        [Test]
        public void NoPinch_DoesNotSelect()
        {
            Assert.IsFalse(HandPinchSelect.ShouldSelect(
                indexPinching: false,
                pinchStrength: 0f,
                triggerButton: false,
                triggerAnalog: 0f,
                menuButton: false));
        }
    }
}
