using System;

namespace MindSilence.Presentation
{
    /// <summary>
    /// Post-splash navigation flags. Visible panel: highscores → training → menu.
    /// <c>InTraining</c> stays true while highscore is shown on top of training.
    /// </summary>
    public readonly struct AppUiState : IEquatable<AppUiState>
    {
        public AppUiState(bool inTraining, bool showHighScores)
        {
            InTraining = inTraining;
            ShowHighScores = showHighScores;
        }

        public bool InTraining { get; }

        public bool ShowHighScores { get; }

        public bool ShowMenu => !ShowHighScores && !InTraining;

        public bool ShowTraining => InTraining && !ShowHighScores;

        public bool Equals(AppUiState other) =>
            InTraining == other.InTraining && ShowHighScores == other.ShowHighScores;

        public override bool Equals(object obj) => obj is AppUiState other && Equals(other);

        public override int GetHashCode() => HashCode.Combine(InTraining, ShowHighScores);

        public static bool operator ==(AppUiState left, AppUiState right) => left.Equals(right);

        public static bool operator !=(AppUiState left, AppUiState right) => !left.Equals(right);
    }
}

