using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit.Interactors.Visuals;

namespace MindSilence.XR
{
    /// <summary>
    /// Maps controller trigger to XRI select and UI press. Thought/Start/Back stay uGUI clicks.
    /// Does not read A/X (primaryButton) or the Meta system menu button (Input.8).
    /// </summary>
    public sealed class ControllerTriggerSelect : MonoBehaviour
    {
        public const float TriggerThreshold = 0.7f;

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

        public static bool ShouldSelect(bool triggerButton, float triggerAnalog, bool menuButton)
        {
            if (menuButton)
            {
                return false;
            }

            return triggerButton || triggerAnalog > TriggerThreshold;
        }

        void Update()
        {
            EnsureInteractor();
            var pressed = TriggerPressed(Node);
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

        static bool TriggerPressed(XRNode xrNode)
        {
            Devices.Clear();
            InputDevices.GetDevicesAtXRNode(xrNode, Devices);
            foreach (var device in Devices)
            {
                if (!device.isValid)
                {
                    continue;
                }

                if ((device.characteristics & InputDeviceCharacteristics.HandTracking) != 0)
                {
                    continue;
                }

                if (device.TryGetFeatureValue(CommonUsages.menuButton, out var menu) && menu)
                {
                    return false;
                }

                var triggerButton = device.TryGetFeatureValue(CommonUsages.triggerButton, out var button) && button;
                var analog = 0f;
                device.TryGetFeatureValue(CommonUsages.trigger, out analog);
                if (ShouldSelect(triggerButton, analog, menuButton: false))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
