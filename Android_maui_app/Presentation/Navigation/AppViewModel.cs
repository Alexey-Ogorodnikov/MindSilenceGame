using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Maui.Storage;

namespace MindSilence.Presentation.Navigation;

/// <summary>
/// Navigation is this state. Child screens do not receive this type;
/// the host calls <see cref="OpenTraining"/>, <see cref="LeaveTraining"/>,
/// <see cref="OpenHighScores"/>, and <see cref="LeaveHighScores"/>.
/// </summary>
public sealed partial class AppViewModel : ObservableObject
{
	public const string InTrainingKey = "in_training";
	public const string ShowHighScoresKey = "show_high_scores";

	private readonly IPreferences _preferences;
	private bool _suppressPersist;

	[ObservableProperty]
	[NotifyPropertyChangedFor(nameof(State))]
	[NotifyPropertyChangedFor(nameof(ShowMenu))]
	[NotifyPropertyChangedFor(nameof(ShowTraining))]
	private bool inTraining;

	[ObservableProperty]
	[NotifyPropertyChangedFor(nameof(State))]
	[NotifyPropertyChangedFor(nameof(ShowMenu))]
	[NotifyPropertyChangedFor(nameof(ShowTraining))]
	private bool showHighScores;

	public AppViewModel(IPreferences preferences)
	{
		_preferences = preferences;
	}

	public AppUiState State => new(InTraining, ShowHighScores);

	public bool ShowMenu => State.ShowMenu;

	public bool ShowTraining => State.ShowTraining;

	public void OpenTraining() => InTraining = true;

	public void LeaveTraining() => InTraining = false;

	public void OpenHighScores() => ShowHighScores = true;

	public void LeaveHighScores() => ShowHighScores = false;

	public void ResetToMenu()
	{
		InTraining = false;
		ShowHighScores = false;
		Persist();
	}

	public void RestoreFromStore()
	{
		_suppressPersist = true;
		try
		{
			InTraining = _preferences.Get(InTrainingKey, false);
			ShowHighScores = _preferences.Get(ShowHighScoresKey, false);
		}
		finally
		{
			_suppressPersist = false;
		}
	}

	partial void OnInTrainingChanged(bool value)
	{
		if (!_suppressPersist)
		{
			Persist();
		}
	}

	partial void OnShowHighScoresChanged(bool value)
	{
		if (!_suppressPersist)
		{
			Persist();
		}
	}

	private void Persist()
	{
		_preferences.Set(InTrainingKey, InTraining);
		_preferences.Set(ShowHighScoresKey, ShowHighScores);
	}
}
