using MindSilence.XR;
using NUnit.Framework;

namespace MindSilence.Tests.EditMode
{
    public sealed class UiInteractionRigTests
    {
        [Test]
        public void LocomotionAndSnapTurn_AreStrippedTypes()
        {
            Assert.IsTrue(UiInteractionRig.IsLocomotionType("ActionBasedSnapTurnProvider"));
            Assert.IsTrue(UiInteractionRig.IsLocomotionType("ContinuousMoveProvider"));
            Assert.IsTrue(UiInteractionRig.IsLocomotionType("TeleportationProvider"));
            Assert.IsTrue(UiInteractionRig.IsLocomotionType("GrabMoveProvider"));
            Assert.IsTrue(UiInteractionRig.IsLocomotionType("TurnProvider"));
            Assert.IsFalse(UiInteractionRig.IsLocomotionType("XRRayInteractor"));
            Assert.IsFalse(UiInteractionRig.IsLocomotionType("XRPokeInteractor"));
            Assert.IsFalse(UiInteractionRig.IsLocomotionType("OVRCameraRig"));
        }
    }
}
