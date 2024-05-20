using WmMobileInventory.MVVM.ViewModels;

namespace WmMobileInventory.MVVM.Pages;

public partial class SelectDeptPage : ContentPage
{
	public SelectDeptPage(SelectDeptPageViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
	}
}