using System;

namespace MindSilence.Domain
{
    public sealed class GameSession : IDisposable
    {
        public const long TickIntervalMs = 1000;

        private readonly IGameProgressRepository _progressRepository;
        private readonly ITimeSource _timeSource;

        private GameSessionState _state;
        private bool _isTicking;
        private long _nextTickAtMs;
        private bool _wasRunningBeforeBackground;
        private bool _disposed;

        public GameSession(IGameProgressRepository progressRepository, ITimeSource timeSource)
        {
            _progressRepository = progressRepository ?? throw new ArgumentNullException(nameof(progressRepository));
            _timeSource = timeSource ?? throw new ArgumentNullException(nameof(timeSource));
            _timeSource.Advanced += OnTimeAdvanced;
        }

        public GameSessionState State => _state;

        public event Action Changed;
        public event Action HapticOnThought;
        public event Action<bool> KeepAwake;
        public event Action NavigateToHighScores;
        public event Action NavigateBackToMenu;

        public void Start()
        {
            ThrowIfDisposed();
            if (_state.Phase == GamePhase.Running)
            {
                return;
            }

            SetState(new GameSessionState(
                GamePhase.Running,
                level: 1,
                elapsedSecAtLevel: 0,
                _state.SessionSummary));
            KeepAwake?.Invoke(true);
            StartTicking();
        }

        public void Thought()
        {
            ThrowIfDisposed();
            if (_state.Phase != GamePhase.Running)
            {
                return;
            }

            var levelReached = _state.Level;
            var totalSeconds = LevelDuration.TotalSessionSeconds(levelReached, _state.ElapsedSecAtLevel);
            var bestToday = _progressRepository.RecordSession(levelReached, totalSeconds);

            StopTicking();
            SetState(new GameSessionState(
                GamePhase.Idle,
                level: 0,
                elapsedSecAtLevel: 0,
                new SessionSummary(levelReached, bestToday, totalSeconds)));
            KeepAwake?.Invoke(false);
            HapticOnThought?.Invoke();
        }

        public void DismissSessionSummary()
        {
            ThrowIfDisposed();
            SetState(new GameSessionState(
                _state.Phase,
                _state.Level,
                _state.ElapsedSecAtLevel,
                sessionSummary: null));
        }

        public void OpenHighScores()
        {
            ThrowIfDisposed();
            SetState(new GameSessionState(
                _state.Phase,
                _state.Level,
                _state.ElapsedSecAtLevel,
                sessionSummary: null));
            NavigateToHighScores?.Invoke();
        }

        public void LeaveTraining()
        {
            ThrowIfDisposed();
            NavigateBackToMenu?.Invoke();
        }

        public void AppBackgrounded()
        {
            ThrowIfDisposed();
            if (_state.Phase != GamePhase.Running)
            {
                return;
            }

            _wasRunningBeforeBackground = true;
            StopTicking();
            KeepAwake?.Invoke(false);
        }

        public void AppForegrounded()
        {
            ThrowIfDisposed();
            if (!_wasRunningBeforeBackground || _state.Phase != GamePhase.Running)
            {
                _wasRunningBeforeBackground = false;
                return;
            }

            _wasRunningBeforeBackground = false;
            KeepAwake?.Invoke(true);
            StartTicking();
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            StopTicking();
            KeepAwake?.Invoke(false);
            _timeSource.Advanced -= OnTimeAdvanced;
        }

        private void StartTicking()
        {
            if (_isTicking)
            {
                return;
            }

            _isTicking = true;
            _nextTickAtMs = _timeSource.ElapsedMilliseconds + TickIntervalMs;
        }

        private void StopTicking()
        {
            _isTicking = false;
        }

        private void OnTimeAdvanced()
        {
            if (_disposed || !_isTicking)
            {
                return;
            }

            while (_isTicking && _timeSource.ElapsedMilliseconds >= _nextTickAtMs)
            {
                _nextTickAtMs += TickIntervalMs;
                AdvanceTick();
            }
        }

        private void AdvanceTick()
        {
            if (_state.Phase != GamePhase.Running)
            {
                return;
            }

            var nextElapsed = _state.ElapsedSecAtLevel + 1;
            var required = _state.RequiredSecAtLevel;
            SetState(nextElapsed >= required
                ? new GameSessionState(GamePhase.Running, _state.Level + 1, 0, _state.SessionSummary)
                : new GameSessionState(GamePhase.Running, _state.Level, nextElapsed, _state.SessionSummary));
        }

        private void SetState(GameSessionState state)
        {
            _state = state;
            Changed?.Invoke();
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(GameSession));
            }
        }
    }
}
