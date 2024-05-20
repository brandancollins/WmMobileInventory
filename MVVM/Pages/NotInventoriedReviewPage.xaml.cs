using WmMobileInventory.MVVM.ViewModels;

namespace WmMobileInventory.MVVM.Pages;

public partial class NotInventoriedReviewPage : ContentPage
{
	public NotInventoriedReviewPage(NotInventoriedReviewPageViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
	}
}