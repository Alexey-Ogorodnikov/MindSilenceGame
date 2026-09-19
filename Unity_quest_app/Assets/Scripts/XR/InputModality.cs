namespace MindSilence.XR
{
    /// <summary>
    /// Hands vs controllers for UI poke/ray. Input.6 hide visuals when untracked
    /// or low confidence; Input.7 switch on the fly; Input.8 does not live here
    /// (system menu is never mapped to a product action).
    /// </summary>
    public readonly struct TrackedHand
    {
        public readonly bool Tracked;
        public readonly bool HighConfidence;

        public TrackedHand(bool tracked, bool highConfidence)
        {
            Tracked = tracked;
            HighConfidence = highConfidence;
        }
    }

    public readonly struct InputModalitySnapshot
    {
        public readonly bool AppPaused;
        public readonly bool LeftController;
        public readonly bool RightController;
        public readonly TrackedHand LeftHand;
        public readonly TrackedHand RightHand;

        public InputModalitySnapshot(
            bool appPaused,
            bool leftController,
            bool rightController,
            TrackedHand leftHand,
            TrackedHand rightHand)
        {
            AppPaused = appPaused;
            LeftController = leftController;
            RightController = rightController;
            LeftHand = leftHand;
            RightHand = rightHand;
        }

        public bool HasHeadsetSignal =>
            LeftController || RightController || LeftHand.Tracked || RightHand.Tracked;
    }

    public readonly struct InputModalityDecision
    {
        public readonly bool LeftPoke;
        public readonly bool RightPoke;
        public readonly bool LeftRay;
        public readonly bool RightRay;
        public readonly bool LeftHandVisual;
        public readonly bool RightHandVisual;
        public readonly bool LeftControllerVisual;
        public readonly bool RightControllerVisual;

        public InputModalityDecision(
            bool leftPoke,
            bool rightPoke,
            bool leftRay,
            bool rightRay,
            bool leftHandVisual,
            bool rightHandVisual,
            bool leftControllerVisual,
            bool rightControllerVisual)
        {
            LeftPoke = leftPoke;
            RightPoke = rightPoke;
            LeftRay = leftRay;
            RightRay = rightRay;
            LeftHandVisual = leftHandVisual;
            RightHandVisual = rightHandVisual;
            LeftControllerVisual = leftControllerVisual;
            RightControllerVisual = rightControllerVisual;
        }
    }

    public static class InputModality
    {
        public static bool ShowHandVisual(
            bool tracked,
            bool highConfidence,
            bool thatSideController,
            bool appPaused)
        {
            if (appPaused)
            {
                return false;
            }

            return tracked && highConfidence && !thatSideController;
        }

        public static InputModalityDecision Decide(InputModalitySnapshot snapshot)
        {
            if (snapshot.AppPaused)
            {
                return new InputModalityDecision(
                    leftPoke: false,
                    rightPoke: false,
                    leftRay: false,
                    rightRay: false,
                    leftHandVisual: false,
                    rightHandVisual: false,
                    leftControllerVisual: false,
                    rightControllerVisual: false);
            }

            if (!snapshot.HasHeadsetSignal)
            {
                // Editor / no headset: keep poke and ray so UI stays clickable.
                return new InputModalityDecision(
                    leftPoke: true,
                    rightPoke: true,
                    leftRay: true,
                    rightRay: true,
                    leftHandVisual: false,
                    rightHandVisual: false,
                    leftControllerVisual: false,
                    rightControllerVisual: false);
            }

            var leftHand = ShowHandVisual(
                snapshot.LeftHand.Tracked,
                snapshot.LeftHand.HighConfidence,
                snapshot.LeftController,
                appPaused: false);
            var rightHand = ShowHandVisual(
                snapshot.RightHand.Tracked,
                snapshot.RightHand.HighConfidence,
                snapshot.RightController,
                appPaused: false);
            return new InputModalityDecision(
                leftPoke: leftHand,
                rightPoke: rightHand,
                leftRay: snapshot.LeftController,
                rightRay: snapshot.RightController,
                leftHandVisual: leftHand,
                rightHandVisual: rightHand,
                leftControllerVisual: snapshot.LeftController,
                rightControllerVisual: snapshot.RightController);
        }
    }
}
