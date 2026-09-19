namespace MindSilence.Presentation.HighScores;

public partial class HighScoresPage : ContentView
{
	public HighScoresViewModel ViewModel { get; }

	public HighScoresPage(HighScoresViewModel viewModel)
	{
		InitializeComponent();
		ViewModel = viewModel;
		BindingContext = viewModel;
	}
}
