using WmMobileInventory.MVVM.ViewModels;

namespace WmMobileInventory.MVVM.Pages;

public partial class CommentsPage : ContentPage
{
	public CommentsPage(CommentPageViewModel vm)
	{
		InitializeComponent();
        BindingContext = vm;
    }

    protected override bool OnBackButtonPressed()
    {
        // Optionally, show a message to inform the user that they can't go back
        // DisplayAlert("Info", "Please use Save or Cancel.", "OK");

        // Return true to prevent the back button from closing the modal
        return true;
    }
}