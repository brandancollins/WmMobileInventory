using WmMobileInventory.MVVM.ViewModels;

namespace WmMobileInventory.MVVM.Pages;

public partial class SelectRoomPage : ContentPage
{
	public SelectRoomPage(SelectRoomPageViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
	}
}