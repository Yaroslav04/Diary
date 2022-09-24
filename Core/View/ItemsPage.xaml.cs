namespace Diary.Core.View;

public partial class ItemsPage : ContentPage
{
    ItemsPageViewModel viewModel;
	public ItemsPage()
	{
		InitializeComponent();
        viewModel = new ItemsPageViewModel();
        BindingContext = viewModel;

	}

    protected override void OnAppearing()
    {
        base.OnAppearing();
        viewModel.OnAppearing();
    }
}