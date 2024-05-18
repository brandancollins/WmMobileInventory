using WmMobileInventory.MVVM.ViewModels;

namespace WmMobileInventory.MVVM.Pages;

public partial class InventoryReviewPage : ContentPage
{
	public InventoryReviewPage(InventoryReviewPageViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
	}
}