using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

namespace MindSilence.XR
{
    /// <summary>
    /// Applies <see cref="InputModality"/> to poke/ray hosts and app hand meshes.
    /// Does not bind the Quest system menu gesture.
    /// </summary>
    public sealed class InputModalityGate : MonoBehaviour
    {
        static readonly List<InputDevice> Devices = new List<InputDevice>();

        bool _appPaused;

        public static void Ensure(Canvas canvas)
        {
            if (canvas == null)
            {
                return;
            }

            if (canvas.GetComponent<InputModalityGate>() == null)
            {
                canvas.gameObject.AddComponent<InputModalityGate>();
            }
        }

        public void NotifyPaused(bool paused)
        {
            _appPaused = paused;
            Apply(InputModality.Decide(ReadSnapshot()));
        }

        public void Apply(InputModalityDecision decision)
        {
            SetActive(UiInteractionRig.LeftPokeName, decision.LeftPoke);
            SetActive(UiInteractionRig.RightPokeName, decision.RightPoke);
            SetActive(UiInteractionRig.LeftRayName, decision.LeftRay);
            SetActive(UiInteractionRig.RightRayName, decision.RightRay);
            // Sitting panel is 1.2–1.5 m; poke cannot reach it. Pinch-ray uses the same hand gate.
            SetActive(UiInteractionRig.LeftHandRayName, decision.LeftPoke);
            SetActive(UiInteractionRig.RightHandRayName, decision.RightPoke);
            SetHandVisuals("LeftHandAnchor", decision.LeftHandVisual);
            SetHandVisuals("RightHandAnchor", decision.RightHandVisual);
            SetControllerVisuals("LeftControllerAnchor", decision.LeftControllerVisual);
            SetControllerVisuals("RightControllerAnchor", decision.RightControllerVisual);
        }

        void Update()
        {
            Apply(InputModality.Decide(ReadSnapshot()));
        }

        void OnApplicationPause(bool pauseStatus)
        {
            NotifyPaused(pauseStatus);
        }

        InputModalitySnapshot ReadSnapshot()
        {
            return new InputModalitySnapshot(
                _appPaused,
                ControllerTracked(XRNode.LeftHand),
                ControllerTracked(XRNode.RightHand),
                ReadHand(XRNode.LeftHand),
                ReadHand(XRNode.RightHand));
        }

        static bool ControllerTracked(XRNode node)
        {
            Devices.Clear();
            InputDevices.GetDevicesAtXRNode(node, Devices);
            foreach (var device in Devices)
            {
                if (!device.isValid)
                {
                    continue;
                }

                var characteristics = device.characteristics;
                if ((characteristics & InputDeviceCharacteristics.Controller) == 0)
                {
                    continue;
                }

                if ((characteristics & InputDeviceCharacteristics.HandTracking) != 0)
                {
                    continue;
                }

                if (device.TryGetFeatureValue(CommonUsages.isTracked, out var tracked) && tracked)
                {
                    return true;
                }
            }

            return false;
        }

        static TrackedHand ReadHand(XRNode node)
        {
            Devices.Clear();
            InputDevices.GetDevicesAtXRNode(node, Devices);
            foreach (var device in Devices)
            {
                if (!device.isValid)
                {
                    continue;
                }

                if ((device.characteristics & InputDeviceCharacteristics.HandTracking) == 0)
                {
                    continue;
                }

                var tracked = device.TryGetFeatureValue(CommonUsages.isTracked, out var isTracked) && isTracked;
                var highConfidence = tracked;
                if (device.TryGetFeatureValue(CommonUsages.trackingState, out InputTrackingState state))
                {
                    highConfidence = tracked
                        && (state & InputTrackingState.Position) != 0
                        && (state & InputTrackingState.Rotation) != 0;
                }

                return new TrackedHand(tracked, highConfidence);
            }

            return new TrackedHand(false, false);
        }

        static void SetActive(string name, bool active)
        {
            var found = FindNamed(name);
            if (found != null && found.gameObject.activeSelf != active)
            {
                found.gameObject.SetActive(active);
            }
        }

        static void SetHandVisuals(string anchorName, bool show)
        {
            var anchor = FindNamed(anchorName);
            if (anchor == null)
            {
                return;
            }

            SetHintedRenderers(anchor, show, "OVRHand", "HandMesh", "HandVisual", "XRHandMesh");
        }

        static void SetControllerVisuals(string anchorName, bool show)
        {
            var anchor = FindNamed(anchorName);
            if (anchor == null)
            {
                return;
            }

            SetHintedRenderers(anchor, show, "OVRController", "Controller");
        }

        static void SetHintedRenderers(Transform root, bool show, params string[] hints)
        {
            var renderers = root.GetComponentsInChildren<Renderer>(true);
            foreach (var renderer in renderers)
            {
                if (renderer == null || renderer is LineRenderer)
                {
                    continue;
                }

                if (!NameMatches(renderer.transform, hints))
                {
                    continue;
                }

                renderer.enabled = show;
            }
        }

        static bool NameMatches(Transform transform, string[] hints)
        {
            var current = transform;
            while (current != null)
            {
                var name = current.name;
                foreach (var hint in hints)
                {
                    if (name.IndexOf(hint, System.StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        return true;
                    }
                }

                current = current.parent;
            }

            return false;
        }

        static Transform FindNamed(string name)
        {
            var found = GameObject.Find(name);
            if (found != null)
            {
                return found.transform;
            }

            foreach (var transform in FindObjectsByType<Transform>(FindObjectsInactive.Include))
            {
                if (transform != null && transform.name == name)
                {
                    return transform;
                }
            }

            return null;
        }
    }
}
