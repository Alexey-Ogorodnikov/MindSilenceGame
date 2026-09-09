namespace MindSilence.Presentation.Navigation;

/// <summary>
/// App-wide navigation flags. Visible page: highscores → training → menu.
/// <c>InTraining</c> stays true while highscore is shown on top of training.
/// </summary>
public readonly record struct AppUiState(bool InTraining, bool ShowHighScores)
{
	public bool ShowMenu => !ShowHighScores && !InTraining;

	public bool ShowTraining => InTraining && !ShowHighScores;
}
