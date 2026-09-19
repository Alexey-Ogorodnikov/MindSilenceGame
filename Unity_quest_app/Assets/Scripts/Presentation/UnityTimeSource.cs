using System;
using MindSilence.Domain;
using UnityEngine;

namespace MindSilence.Presentation
{
    /// <summary>
    /// Runtime clock. Host calls <see cref="NotifyAdvanced"/> from Update; tests use FakeTimeSource.
    /// </summary>
    public sealed class UnityTimeSource : ITimeSource
    {
        public long ElapsedMilliseconds => (long)(Time.realtimeSinceStartupAsDouble * 1000.0);

        public event Action Advanced;

        public void NotifyAdvanced() => Advanced?.Invoke();
    }
}
