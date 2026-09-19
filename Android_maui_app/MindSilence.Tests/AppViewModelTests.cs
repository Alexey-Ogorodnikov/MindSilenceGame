using MindSilence.Presentation.Navigation;

namespace MindSilence.Tests;

public sealed class AppViewModelTests
{
	[Fact]
	public void OpenTraining_enters_training()
	{
		var viewModel = new AppViewModel(new MemoryPreferences());

		viewModel.OpenTraining();

		Assert.True(viewModel.InTraining);
		Assert.False(viewModel.ShowHighScores);
		Assert.True(viewModel.ShowTraining);
		Assert.False(viewModel.ShowMenu);
	}

	[Fact]
	public void LeaveTraining_returns_to_menu()
	{
		var viewModel = new AppViewModel(new MemoryPreferences());
		viewModel.OpenTraining();

		viewModel.LeaveTraining();

		Assert.False(viewModel.InTraining);
		Assert.False(viewModel.ShowHighScores);
		Assert.True(viewModel.ShowMenu);
	}

	[Fact]
	public void OpenHighScores_does_not_clear_in_training()
	{
		var viewModel = new AppViewModel(new MemoryPreferences());
		viewModel.OpenTraining();

		viewModel.OpenHighScores();

		Assert.True(viewModel.InTraining);
		Assert.True(viewModel.ShowHighScores);
		Assert.False(viewModel.ShowTraining);
		Assert.False(viewModel.ShowMenu);
		Assert.Equal(new AppUiState(InTraining: true, ShowHighScores: true), viewModel.State);
	}

	[Fact]
	public void LeaveHighScores_returns_to_training()
	{
		var viewModel = new AppViewModel(new MemoryPreferences());
		viewModel.OpenTraining();
		viewModel.OpenHighScores();

		viewModel.LeaveHighScores();

		Assert.True(viewModel.InTraining);
		Assert.False(viewModel.ShowHighScores);
		Assert.True(viewModel.ShowTraining);
	}

	[Fact]
	public void RestoreFromStore_reads_persisted_flags()
	{
		var store = new MemoryPreferences();
		store.Set(AppViewModel.InTrainingKey, true);
		store.Set(AppViewModel.ShowHighScoresKey, true);

		var viewModel = new AppViewModel(store);
		viewModel.RestoreFromStore();

		Assert.Equal(new AppUiState(InTraining: true, ShowHighScores: true), viewModel.State);
	}

	[Fact]
	public void Navigation_methods_persist_flags()
	{
		var store = new MemoryPreferences();
		var viewModel = new AppViewModel(store);

		viewModel.OpenTraining();
		viewModel.OpenHighScores();

		Assert.True(store.Get(AppViewModel.InTrainingKey, false));
		Assert.True(store.Get(AppViewModel.ShowHighScoresKey, false));
	}
}
