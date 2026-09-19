namespace MindSilence.Presentation.Menu;

public partial class MenuPage : ContentView
{
	public MenuViewModel ViewModel { get; }

	public MenuPage(MenuViewModel viewModel)
	{
		InitializeComponent();
		ViewModel = viewModel;
		BindingContext = viewModel;
	}
}
