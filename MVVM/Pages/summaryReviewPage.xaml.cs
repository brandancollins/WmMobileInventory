using WmMobileInventory.MVVM.ViewModels;

namespace WmMobileInventory.MVVM.Pages;

public partial class SummaryReviewPage : ContentPage
{
	public SummaryReviewPage(SummaryReviewPageViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
	}
}