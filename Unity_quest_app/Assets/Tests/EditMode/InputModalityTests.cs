using MindSilence.XR;
using NUnit.Framework;

namespace MindSilence.Tests.EditMode
{
    public sealed class InputModalityTests
    {
        [Test]
        public void LowConfidenceOrLostTracking_HidesHandAndDisablesPoke()
        {
            var lost = InputModality.Decide(new InputModalitySnapshot(
                appPaused: false,
                leftController: false,
                rightController: false,
                leftHand: new TrackedHand(tracked: false, highConfidence: false),
                rightHand: new TrackedHand(tracked: true, highConfidence: false)));

            Assert.IsFalse(lost.LeftHandVisual);
            Assert.IsFalse(lost.RightHandVisual);
            Assert.IsFalse(lost.LeftPoke);
            Assert.IsFalse(lost.RightPoke);
        }

        [Test]
        public void HighConfidenceHands_EnablePokeAndHideRays()
        {
            var hands = InputModality.Decide(new InputModalitySnapshot(
                appPaused: false,
                leftController: false,
                rightController: false,
                leftHand: new TrackedHand(true, true),
                rightHand: new TrackedHand(true, true)));

            Assert.IsTrue(hands.LeftPoke);
            Assert.IsTrue(hands.RightPoke);
            Assert.IsTrue(hands.LeftHandVisual);
            Assert.IsTrue(hands.RightHandVisual);
            Assert.IsFalse(hands.LeftRay);
            Assert.IsFalse(hands.RightRay);
        }

        [Test]
        public void Controllers_EnableRaysAndHideHandsWithoutRestart()
        {
            var controllers = InputModality.Decide(new InputModalitySnapshot(
                appPaused: false,
                leftController: true,
                rightController: true,
                leftHand: new TrackedHand(true, true),
                rightHand: new TrackedHand(true, true)));

            Assert.IsTrue(controllers.LeftRay);
            Assert.IsTrue(controllers.RightRay);
            Assert.IsFalse(controllers.LeftPoke);
            Assert.IsFalse(controllers.RightPoke);
            Assert.IsFalse(controllers.LeftHandVisual);
            Assert.IsFalse(controllers.RightHandVisual);
            Assert.IsTrue(controllers.LeftControllerVisual);
            Assert.IsTrue(controllers.RightControllerVisual);

            var handsAgain = InputModality.Decide(new InputModalitySnapshot(
                appPaused: false,
                leftController: false,
                rightController: false,
                leftHand: new TrackedHand(true, true),
                rightHand: new TrackedHand(true, true)));

            Assert.IsTrue(handsAgain.LeftPoke);
            Assert.IsFalse(handsAgain.LeftRay);
        }

        [Test]
        public void Mixed_LeftControllerRightHand_WorksPerSide()
        {
            var mixed = InputModality.Decide(new InputModalitySnapshot(
                appPaused: false,
                leftController: true,
                rightController: false,
                leftHand: new TrackedHand(true, true),
                rightHand: new TrackedHand(true, true)));

            Assert.IsTrue(mixed.LeftRay);
            Assert.IsFalse(mixed.LeftPoke);
            Assert.IsFalse(mixed.LeftHandVisual);
            Assert.IsTrue(mixed.RightPoke);
            Assert.IsFalse(mixed.RightRay);
            Assert.IsTrue(mixed.RightHandVisual);
        }

        [Test]
        public void Pause_HidesHandsAndIgnoresPokeAndRay()
        {
            var paused = InputModality.Decide(new InputModalitySnapshot(
                appPaused: true,
                leftController: true,
                rightController: false,
                leftHand: new TrackedHand(true, true),
                rightHand: new TrackedHand(true, true)));

            Assert.IsFalse(paused.LeftPoke);
            Assert.IsFalse(paused.RightPoke);
            Assert.IsFalse(paused.LeftRay);
            Assert.IsFalse(paused.RightRay);
            Assert.IsFalse(paused.LeftHandVisual);
            Assert.IsFalse(paused.RightHandVisual);
        }

        [Test]
        public void NoHeadsetSignal_KeepsPokeAndRayForEditor()
        {
            var editor = InputModality.Decide(new InputModalitySnapshot(
                appPaused: false,
                leftController: false,
                rightController: false,
                leftHand: new TrackedHand(false, false),
                rightHand: new TrackedHand(false, false)));

            Assert.IsTrue(editor.LeftPoke);
            Assert.IsTrue(editor.RightPoke);
            Assert.IsTrue(editor.LeftRay);
            Assert.IsTrue(editor.RightRay);
            Assert.IsFalse(editor.LeftHandVisual);
            Assert.IsFalse(editor.RightHandVisual);
        }
    }
}
