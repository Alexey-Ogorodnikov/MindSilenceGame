using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace MindSilence.Presentation.Menu;

/// <summary>
/// Menu: How-to-train dialog visibility in state. Opening training is a
/// one-shot <see cref="NavigateToTraining"/> effect so this screen never talks to AppViewModel.
/// </summary>
public sealed partial class MenuViewModel : ObservableObject
{
	[ObservableProperty]
	private bool showHowToTrain;

	public event EventHandler? NavigateToTraining;

	[RelayCommand]
	public void OnOpenTraining() => NavigateToTraining?.Invoke(this, EventArgs.Empty);

	[RelayCommand]
	public void OnOpenHowToTrain() => ShowHowToTrain = true;

	[RelayCommand]
	public void OnDismissHowToTrain() => ShowHowToTrain = false;
}
