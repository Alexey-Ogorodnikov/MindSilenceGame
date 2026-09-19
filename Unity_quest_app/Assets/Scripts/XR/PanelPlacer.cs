using UnityEngine;

namespace MindSilence.XR
{
    /// <summary>
    /// Places a world-space panel once in front of the seated user.
    /// Does not parent to the camera or follow the head every frame.
    /// </summary>
    [DefaultExecutionOrder(100)]
    public sealed class PanelPlacer : MonoBehaviour
    {
        public const float DistanceMetersDefault = 1.35f;
        public const float BelowEyesMetersDefault = 0.12f;

        [SerializeField] float distanceMeters = DistanceMetersDefault;
        [SerializeField] float belowEyesMeters = BelowEyesMetersDefault;

        bool _placed;

        void Start()
        {
            StartCoroutine(PlaceAfterTracking());
        }

        System.Collections.IEnumerator PlaceAfterTracking()
        {
            for (var frame = 0; frame < 90 && !_placed; frame++)
            {
                yield return null;
                PlaceOnce();
            }
        }

        void PlaceOnce()
        {
            if (_placed)
            {
                return;
            }

            var eye = FindEyeTransform();
            if (eye == null)
            {
                return;
            }

            var forward = eye.forward;
            forward.y = 0f;
            if (forward.sqrMagnitude < 0.0001f)
            {
                return;
            }

            forward.Normalize();
            transform.SetParent(null, true);
            transform.position = eye.position + (forward * distanceMeters) + (Vector3.down * belowEyesMeters);
            // World-space uGUI is drawn on local XY. The readable face is -Z
            // (same as Unity's default camera at z=-10 looking +Z). UI/Default
            // has Cull Off, so the +Z face shows mirrored text. Point +Z the
            // way the user looks so they see the -Z face.
            transform.rotation = Quaternion.LookRotation(forward, Vector3.up);
            _placed = true;
        }

        static Transform FindEyeTransform()
        {
            if (Camera.main != null)
            {
                return Camera.main.transform;
            }

            var cameras = FindObjectsByType<Camera>(FindObjectsSortMode.None);
            foreach (var camera in cameras)
            {
                if (camera.CompareTag("MainCamera") || camera.name.Contains("CenterEye"))
                {
                    return camera.transform;
                }
            }

            return cameras.Length > 0 ? cameras[0].transform : null;
        }
    }
}
