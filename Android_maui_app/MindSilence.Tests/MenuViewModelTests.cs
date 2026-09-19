using MindSilence.Presentation.Menu;

namespace MindSilence.Tests;

public sealed class MenuViewModelTests
{
	[Fact]
	public void OpenHowToTrain_shows_dialog()
	{
		var viewModel = new MenuViewModel();

		viewModel.OnOpenHowToTrain();

		Assert.True(viewModel.ShowHowToTrain);
	}

	[Fact]
	public void DismissHowToTrain_hides_dialog()
	{
		var viewModel = new MenuViewModel();
		viewModel.OnOpenHowToTrain();

		viewModel.OnDismissHowToTrain();

		Assert.False(viewModel.ShowHowToTrain);
	}

	[Fact]
	public void DismissHowToTrain_when_already_hidden_is_noop()
	{
		var viewModel = new MenuViewModel();

		viewModel.OnDismissHowToTrain();

		Assert.False(viewModel.ShowHowToTrain);
	}

	[Fact]
	public void OpenTraining_emits_navigate_effect()
	{
		var viewModel = new MenuViewModel();
		var raised = 0;
		viewModel.NavigateToTraining += (_, _) => raised++;

		viewModel.OnOpenTraining();

		Assert.Equal(1, raised);
	}
}
