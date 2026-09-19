using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Interactors.Visuals;
using UnityEngine.XR.Interaction.Toolkit.UI;
using UnityEngine.UI;

namespace MindSilence.XR
{
    /// <summary>
    /// Poke (hands) and ray+trigger (controllers) for world-space uGUI.
    /// Hands also get a pinch-ray so sitting distance (1.2–1.5 m) is clickable.
    /// Does not add locomotion, snap-turn, or a Meta-menu intercept.
    /// </summary>
    public sealed class UiInteractionRig : MonoBehaviour
    {
        public const string ManagerName = "XR Interaction Manager";
        public const string EventSystemName = "EventSystem";
        public const string LeftRayName = "LeftUiRay";
        public const string RightRayName = "RightUiRay";
        public const string LeftHandRayName = "LeftHandUiRay";
        public const string RightHandRayName = "RightHandUiRay";
        public const string LeftPokeName = "LeftUiPoke";
        public const string RightPokeName = "RightUiPoke";

        public static void Ensure(Canvas canvas)
        {
            if (canvas == null)
            {
                return;
            }

            AssignEventCamera(canvas);
            EnsureInteractionManager();
            EnsureEventSystem();
            EnsureTrackedDeviceGraphicRaycaster(canvas);
            EnsureControllerRay("LeftControllerAnchor", LeftRayName, XRNode.LeftHand);
            EnsureControllerRay("RightControllerAnchor", RightRayName, XRNode.RightHand);
            EnsureHandRay("LeftHandAnchor", LeftHandRayName, XRNode.LeftHand);
            EnsureHandRay("RightHandAnchor", RightHandRayName, XRNode.RightHand);
            EnsureHandPoke("LeftHandAnchor", LeftPokeName);
            EnsureHandPoke("RightHandAnchor", RightPokeName);
            InputModalityGate.Ensure(canvas);
            DestroyLocomotion();
        }

        static void AssignEventCamera(Canvas canvas)
        {
            var center = GameObject.Find("CenterEyeAnchor");
            if (center != null)
            {
                var eye = center.GetComponent<Camera>();
                if (eye != null)
                {
                    canvas.worldCamera = eye;
                    return;
                }
            }

            if (canvas.worldCamera == null)
            {
                canvas.worldCamera = Camera.main;
            }
        }

        static void EnsureInteractionManager()
        {
            if (GameObject.Find(ManagerName) != null)
            {
                return;
            }

            var host = new GameObject(ManagerName);
            host.AddComponent<XRInteractionManager>();
        }

        static void EnsureEventSystem()
        {
            var eventSystem = EventSystem.current != null
                ? EventSystem.current.gameObject
                : GameObject.Find(EventSystemName);
            if (eventSystem == null)
            {
                eventSystem = new GameObject(EventSystemName);
                eventSystem.AddComponent<EventSystem>();
            }

            if (eventSystem.GetComponent<XRUIInputModule>() == null)
            {
                eventSystem.AddComponent<XRUIInputModule>();
            }

            var standalone = eventSystem.GetComponent<StandaloneInputModule>();
            if (standalone != null)
            {
                standalone.enabled = false;
            }

            foreach (var module in eventSystem.GetComponents<BaseInputModule>())
            {
                if (module != null && module is not XRUIInputModule)
                {
                    module.enabled = false;
                }
            }
        }

        static void EnsureTrackedDeviceGraphicRaycaster(Canvas canvas)
        {
            if (canvas.GetComponent<TrackedDeviceGraphicRaycaster>() == null)
            {
                canvas.gameObject.AddComponent<TrackedDeviceGraphicRaycaster>();
            }

            var graphic = canvas.GetComponent<GraphicRaycaster>();
            if (graphic != null)
            {
                graphic.ignoreReversedGraphics = true;
            }
        }

        static void EnsureControllerRay(string anchorName, string rayName, XRNode node)
        {
            var ray = EnsureChild(anchorName, rayName, Vector3.zero);
            if (ray == null)
            {
                return;
            }

            ConfigureRay(ray.gameObject);
            var select = ray.GetComponent<ControllerTriggerSelect>()
                ?? ray.gameObject.AddComponent<ControllerTriggerSelect>();
            select.Node = node;
        }

        static void EnsureHandRay(string anchorName, string rayName, XRNode node)
        {
            var attach = FindHandAim(FindNamed(anchorName)) ?? FindNamed(anchorName);
            if (attach == null)
            {
                return;
            }

            var ray = attach.Find(rayName);
            if (ray == null)
            {
                var go = new GameObject(rayName);
                go.transform.SetParent(attach, false);
                go.transform.localPosition = Vector3.zero;
                go.transform.localRotation = Quaternion.identity;
                ray = go.transform;
            }

            ConfigureRay(ray.gameObject);
            var pinch = ray.GetComponent<HandPinchSelect>()
                ?? ray.gameObject.AddComponent<HandPinchSelect>();
            pinch.Node = node;
        }

        static Transform FindHandAim(Transform handAnchor)
        {
            if (handAnchor == null)
            {
                return null;
            }

            foreach (var child in handAnchor.GetComponentsInChildren<Transform>(true))
            {
                if (child == null)
                {
                    continue;
                }

                var name = child.name;
                if (name.IndexOf("PointerPose", StringComparison.OrdinalIgnoreCase) >= 0
                    || name.IndexOf("HandAim", StringComparison.OrdinalIgnoreCase) >= 0
                    || string.Equals(name, "AimPose", StringComparison.OrdinalIgnoreCase))
                {
                    return child;
                }
            }

            return handAnchor;
        }

        static Transform EnsureChild(string anchorName, string childName, Vector3 localPosition)
        {
            var anchor = FindNamed(anchorName);
            if (anchor == null)
            {
                return null;
            }

            var child = anchor.Find(childName);
            if (child != null)
            {
                return child;
            }

            var go = new GameObject(childName);
            go.transform.SetParent(anchor, false);
            go.transform.localPosition = localPosition;
            go.transform.localRotation = Quaternion.identity;
            return go.transform;
        }

        static void EnsureHandPoke(string anchorName, string pokeName)
        {
            var poke = EnsureChild(anchorName, pokeName, new Vector3(0f, 0f, 0.08f));
            if (poke == null)
            {
                return;
            }

            var pokeInteractor = poke.GetComponent<XRPokeInteractor>()
                ?? poke.gameObject.AddComponent<XRPokeInteractor>();
            pokeInteractor.enableUIInteraction = true;
            SetBool(pokeInteractor, "requirePokeFilter", false);
        }

        static void ConfigureRay(GameObject ray)
        {
            var interactor = ray.GetComponent<XRRayInteractor>()
                ?? ray.AddComponent<XRRayInteractor>();
            interactor.enableUIInteraction = true;
            interactor.maxRaycastDistance = 10f;
            XrUiButtonReaders.ConfigureManual(interactor);

            var line = ray.GetComponent<LineRenderer>() ?? ray.AddComponent<LineRenderer>();
            line.positionCount = 2;
            line.useWorldSpace = true;
            var visual = ray.GetComponent<XRInteractorLineVisual>()
                ?? ray.AddComponent<XRInteractorLineVisual>();
            QuestPointerVisual.Apply(line, visual);
        }

        static void SetBool(object target, string name, bool value)
        {
            var property = target.GetType().GetProperty(name);
            if (property != null && property.PropertyType == typeof(bool) && property.CanWrite)
            {
                property.SetValue(target, value);
                return;
            }

            var field = target.GetType().GetField(
                name,
                System.Reflection.BindingFlags.Instance
                | System.Reflection.BindingFlags.Public
                | System.Reflection.BindingFlags.NonPublic);
            if (field != null && field.FieldType == typeof(bool))
            {
                field.SetValue(target, value);
            }
        }

        public static bool IsLocomotionType(string typeName)
        {
            if (string.IsNullOrEmpty(typeName))
            {
                return false;
            }

            return typeName.IndexOf("Locomotion", StringComparison.OrdinalIgnoreCase) >= 0
                || typeName.IndexOf("SnapTurn", StringComparison.OrdinalIgnoreCase) >= 0
                || typeName.IndexOf("Teleport", StringComparison.OrdinalIgnoreCase) >= 0
                || typeName.IndexOf("ContinuousMove", StringComparison.OrdinalIgnoreCase) >= 0
                || typeName.IndexOf("ContinuousTurn", StringComparison.OrdinalIgnoreCase) >= 0
                || typeName.IndexOf("TurnProvider", StringComparison.OrdinalIgnoreCase) >= 0
                || typeName.IndexOf("MoveProvider", StringComparison.OrdinalIgnoreCase) >= 0
                || typeName.IndexOf("GrabMove", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        static void DestroyLocomotion()
        {
            foreach (var behaviour in UnityEngine.Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include))
            {
                if (behaviour == null || !IsLocomotionType(behaviour.GetType().Name))
                {
                    continue;
                }

                if (Application.isPlaying)
                {
                    UnityEngine.Object.Destroy(behaviour);
                }
                else
                {
                    UnityEngine.Object.DestroyImmediate(behaviour);
                }
            }
        }

        static Transform FindNamed(string name)
        {
            var found = GameObject.Find(name);
            if (found != null)
            {
                return found.transform;
            }

            foreach (var transform in UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsInactive.Include))
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
