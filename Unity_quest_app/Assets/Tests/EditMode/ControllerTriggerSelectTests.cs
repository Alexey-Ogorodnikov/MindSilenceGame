using MindSilence.XR;
using NUnit.Framework;

namespace MindSilence.Tests.EditMode
{
    public sealed class ControllerTriggerSelectTests
    {
        [Test]
        public void Trigger_Selects()
        {
            Assert.IsTrue(ControllerTriggerSelect.ShouldSelect(
                triggerButton: true,
                triggerAnalog: 0f,
                menuButton: false));
            Assert.IsTrue(ControllerTriggerSelect.ShouldSelect(
                triggerButton: false,
                triggerAnalog: 0.8f,
                menuButton: false));
        }

        [Test]
        public void MenuButton_DoesNotSelect()
        {
            Assert.IsFalse(ControllerTriggerSelect.ShouldSelect(
                triggerButton: true,
                triggerAnalog: 1f,
                menuButton: true));
        }

        [Test]
        public void PrimaryButtonAlone_DoesNotSelect()
        {
            Assert.IsFalse(ControllerTriggerSelect.ShouldSelect(
                triggerButton: false,
                triggerAnalog: 0f,
                menuButton: false));
        }
    }
}
