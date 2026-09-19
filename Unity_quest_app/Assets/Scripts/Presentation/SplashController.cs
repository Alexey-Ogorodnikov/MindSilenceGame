using System;
using MindSilence.Domain;

namespace MindSilence.Presentation
{
    /// <summary>
    /// Branded-splash timing. The host reads ShowBrandedSplash; KeepSystemSplash is the
    /// analog of Kotlin keepSystemSplash. No navigation effects.
    /// </summary>
    public sealed class SplashController : IDisposable
    {
        readonly ITimeSource _time;
        readonly long _startedAt;
        bool _listening;
        bool _disposed;

        public SplashController(bool startWithBrandedSplash, ITimeSource time)
        {
            _time = time ?? throw new ArgumentNullException(nameof(time));
            ShowBrandedSplash = startWithBrandedSplash;
            KeepSystemSplash = startWithBrandedSplash;
            if (!startWithBrandedSplash)
            {
                return;
            }

            _startedAt = time.ElapsedMilliseconds;
            _time.Advanced += OnAdvanced;
            _listening = true;
            OnAdvanced();
        }

        public bool ShowBrandedSplash { get; private set; }

        public bool KeepSystemSplash { get; private set; }

        public event Action Changed;

        public void OnContentMeasured()
        {
            ThrowIfDisposed();
            if (!KeepSystemSplash)
            {
                return;
            }

            KeepSystemSplash = false;
            Changed?.Invoke();
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            StopListening();
        }

        void OnAdvanced()
        {
            if (_disposed || !ShowBrandedSplash)
            {
                return;
            }

            if (_time.ElapsedMilliseconds - _startedAt < SplashDefaults.DurationMs)
            {
                return;
            }

            ShowBrandedSplash = false;
            KeepSystemSplash = false;
            StopListening();
            Changed?.Invoke();
        }

        void StopListening()
        {
            if (!_listening)
            {
                return;
            }

            _time.Advanced -= OnAdvanced;
            _listening = false;
        }

        void ThrowIfDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(SplashController));
            }
        }
    }
}
