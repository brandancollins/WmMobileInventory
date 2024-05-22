using WmMobileInventory.MVVM.ViewModels;

namespace WmMobileInventory.MVVM.Pages;

public partial class AssetDetailsPage : ContentPage
{
	public AssetDetailsPage(AssetDetailsPageViewModel vm)
	{
		InitializeComponent();
        BindingContext = vm;
    }
}