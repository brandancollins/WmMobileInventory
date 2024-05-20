using WmMobileInventory.MVVM.ViewModels;

namespace WmMobileInventory.MVVM.Pages;

public partial class InventoriedReviewPage : ContentPage
{
	public InventoriedReviewPage(InventoriedReviewPageViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
	}
}