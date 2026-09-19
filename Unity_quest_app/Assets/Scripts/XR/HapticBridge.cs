using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

namespace MindSilence.XR
{
    /// <summary>
    /// Short controller rumble on Thought (LongPress analog). Hands have no haptic
    /// and must not invent a sound. Per Meta docs: OVRInput.SetControllerVibration.
    /// </summary>
    public sealed class HapticBridge : MonoBehaviour
    {
        public const float ThoughtAmplitude = 0.7f;
        public const float ThoughtDurationSeconds = 0.2f;

        static readonly List<InputDevice> Devices = new List<InputDevice>();

        Coroutine _stopOvr;

        public void PlayThought()
        {
            SetOvrVibration(ThoughtAmplitude);
            SendXrImpulse(ThoughtAmplitude, ThoughtDurationSeconds);
            if (!Application.isPlaying || !isActiveAndEnabled)
            {
                SetOvrVibration(0f);
                return;
            }

            if (_stopOvr != null)
            {
                StopCoroutine(_stopOvr);
            }

            _stopOvr = StartCoroutine(StopOvrAfterDelay());
        }

        IEnumerator StopOvrAfterDelay()
        {
            yield return new WaitForSecondsRealtime(ThoughtDurationSeconds);
            SetOvrVibration(0f);
            _stopOvr = null;
        }

        static void SendXrImpulse(float amplitude, float durationSeconds)
        {
            Devices.Clear();
            InputDevices.GetDevicesWithCharacteristics(
                InputDeviceCharacteristics.Controller | InputDeviceCharacteristics.HeldInHand,
                Devices);
            foreach (var device in Devices)
            {
                if (device.isValid)
                {
                    device.SendHapticImpulse(0, amplitude, durationSeconds);
                }
            }
        }

        static void SetOvrVibration(float amplitude)
        {
            var ovrInput = FindType("OVRInput");
            if (ovrInput == null)
            {
                return;
            }

            var controllerType = ovrInput.GetNestedType("Controller");
            var method = ovrInput.GetMethod(
                "SetControllerVibration",
                new[] { typeof(float), typeof(float), controllerType });
            if (method == null || controllerType == null)
            {
                return;
            }

            try
            {
                var left = Enum.Parse(controllerType, "LTouch");
                var right = Enum.Parse(controllerType, "RTouch");
                method.Invoke(null, new object[] { 1f, amplitude, left });
                method.Invoke(null, new object[] { 1f, amplitude, right });
            }
            catch (Exception)
            {
                // Hands-only or missing runtime: no rumble, no sound.
            }
        }

        static Type FindType(string fullOrShortName)
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type type = null;
                try
                {
                    type = assembly.GetType(fullOrShortName);
                    if (type == null)
                    {
                        foreach (var candidate in assembly.GetTypes())
                        {
                            if (candidate.Name == fullOrShortName || candidate.FullName == fullOrShortName)
                            {
                                type = candidate;
                                break;
                            }
                        }
                    }
                }
                catch
                {
                    // Dynamic assemblies can throw on GetTypes.
                }

                if (type != null)
                {
                    return type;
                }
            }

            return null;
        }
    }
}
