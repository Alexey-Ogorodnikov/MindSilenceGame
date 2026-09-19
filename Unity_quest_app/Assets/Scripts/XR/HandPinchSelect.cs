using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit.Interactors.Visuals;

namespace MindSilence.XR
{
    /// <summary>
    /// Pinch (or hand trigger) to UI press on a hand ray. Does not bind the Quest menu gesture.
    /// </summary>
    public sealed class HandPinchSelect : MonoBehaviour
    {
        public const float PinchThreshold = 0.7f;

        static readonly List<InputDevice> Devices = new List<InputDevice>();

        [SerializeField] XRNode node = XRNode.RightHand;

        public XRNode Node
        {
            get => node;
            set => node = value;
        }

        Component _interactor;
        XRInteractorLineVisual _lineVisual;
        bool _lookedUp;

        public static bool ShouldSelect(
            bool indexPinching,
            float pinchStrength,
            bool triggerButton,
            float triggerAnalog,
            bool menuButton)
        {
            if (menuButton)
            {
                return false;
            }

            return indexPinching
                || pinchStrength > PinchThreshold
                || triggerButton
                || triggerAnalog > PinchThreshold;
        }

        void Update()
        {
            EnsureInteractor();
            var pressed = PinchPressed(Node, transform);
            XrUiButtonReaders.Queue(_interactor, pressed);
            QuestPointerVisual.SetSelecting(_lineVisual, pressed);
        }

        void EnsureInteractor()
        {
            if (_lookedUp)
            {
                return;
            }

            _lookedUp = true;
            _interactor = GetComponent("XRRayInteractor") as Component;
            _lineVisual = GetComponent<XRInteractorLineVisual>();
            XrUiButtonReaders.ConfigureManual(_interactor);
        }

        static bool PinchPressed(XRNode xrNode, Transform host)
        {
            if (OvrIndexPinching(host))
            {
                return true;
            }

            Devices.Clear();
            InputDevices.GetDevicesAtXRNode(xrNode, Devices);
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

                if (device.TryGetFeatureValue(CommonUsages.menuButton, out var menu) && menu)
                {
                    return false;
                }

                var pinchStrength = ReadFloat(device, "Pinch", "IndexPinch", "PinchStrength");
                var indexPinching = ReadBool(device, "IndexPinching", "Pinch");
                var triggerButton = device.TryGetFeatureValue(CommonUsages.triggerButton, out var button) && button;
                var analog = 0f;
                device.TryGetFeatureValue(CommonUsages.trigger, out analog);
                if (ShouldSelect(indexPinching, pinchStrength, triggerButton, analog, menuButton: false))
                {
                    return true;
                }
            }

            return false;
        }

        static bool OvrIndexPinching(Transform host)
        {
            if (host == null)
            {
                return false;
            }

            var hand = FindNamedComponent(host, "OVRHand");
            if (hand == null)
            {
                return false;
            }

            var method = hand.GetType().GetMethod("GetFingerIsPinching", new[] { typeof(int) });
            if (method == null)
            {
                var enumParam = hand.GetType().GetNestedType("HandFinger");
                method = enumParam != null
                    ? hand.GetType().GetMethod("GetFingerIsPinching", new[] { enumParam })
                    : null;
                if (method == null || enumParam == null)
                {
                    return false;
                }

                try
                {
                    var index = System.Enum.Parse(enumParam, "Index");
                    return (bool)method.Invoke(hand, new[] { index });
                }
                catch
                {
                    return false;
                }
            }

            try
            {
                return (bool)method.Invoke(hand, new object[] { 1 });
            }
            catch
            {
                return false;
            }
        }

        static Component FindNamedComponent(Transform host, string typeName)
        {
            var current = host;
            while (current != null)
            {
                var found = current.GetComponent(typeName) as Component;
                if (found != null)
                {
                    return found;
                }

                current = current.parent;
            }

            return null;
        }

        static float ReadFloat(InputDevice device, params string[] names)
        {
            foreach (var name in names)
            {
                if (device.TryGetFeatureValue(new InputFeatureUsage<float>(name), out var value))
                {
                    return value;
                }
            }

            return 0f;
        }

        static bool ReadBool(InputDevice device, params string[] names)
        {
            foreach (var name in names)
            {
                if (device.TryGetFeatureValue(new InputFeatureUsage<bool>(name), out var value) && value)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
