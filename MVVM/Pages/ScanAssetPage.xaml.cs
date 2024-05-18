using WmMobileInventory.MVVM.ViewModels;

namespace WmMobileInventory.MVVM.Pages;

public partial class ScanAssetPage : ContentPage
{
	public ScanAssetPage(ScanAssetPageViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
	}
}