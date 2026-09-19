using System;
using UnityEngine;

namespace MindSilence.XR
{
    /// <summary>
    /// Quest system menu / headset off maps to domain background/foreground.
    /// Does not treat the Meta menu gesture as LeaveTraining (Input.8).
    /// </summary>
    public sealed class LifecycleBridge : MonoBehaviour
    {
        public event Action<bool> ApplicationPaused;

        bool _paused;
        bool _unfocused;
        bool? _emittedBackgrounded;

        public void NotifyPause(bool paused) => OnApplicationPause(paused);

        public void NotifyFocus(bool hasFocus) => OnApplicationFocus(hasFocus);

        void OnApplicationPause(bool pauseStatus)
        {
            _paused = pauseStatus;
            Emit();
        }

        void OnApplicationFocus(bool hasFocus)
        {
            _unfocused = !hasFocus;
            Emit();
        }

        void Emit()
        {
            var backgrounded = _paused || _unfocused;
            if (_emittedBackgrounded == backgrounded)
            {
                return;
            }

            _emittedBackgrounded = backgrounded;
            ApplicationPaused?.Invoke(backgrounded);
        }
    }
}
