using WmMobileInventory.MVVM.ViewModels;

namespace WmMobileInventory.MVVM.Pages;

public partial class ScanAssetPage : ContentPage
{
	public ScanAssetPage(ScanAssetPageViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;        
		vm.ScanCompleted += OnScanCompleted;
    }

    private async void OnScanCompleted(object sender, EventArgs e)
    {
        await Task.Delay(100); // Small delay to ensure the async task has completed
        BarcodeEntry.Focus();
        BarcodeEntry.CursorPosition = 0;
        BarcodeEntry.SelectionLength = BarcodeEntry.Text?.Length ?? 0;
    }

}