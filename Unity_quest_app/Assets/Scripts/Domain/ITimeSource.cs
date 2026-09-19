using System;

namespace MindSilence.Domain
{
    public interface ITimeSource
    {
        long ElapsedMilliseconds { get; }

        event Action Advanced;
    }

    public sealed class FakeTimeSource : ITimeSource
    {
        public long ElapsedMilliseconds { get; private set; }

        public event Action Advanced;

        public void Advance(TimeSpan delta)
        {
            if (delta < TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException(nameof(delta));
            }

            if (delta == TimeSpan.Zero)
            {
                return;
            }

            ElapsedMilliseconds += (long)delta.TotalMilliseconds;
            Advanced?.Invoke();
        }
    }
}
