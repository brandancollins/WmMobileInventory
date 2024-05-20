using AndroidX.Navigation;
using WmMobileInventory.MVVM.ViewModels;

namespace WmMobileInventory.MVVM.Pages;

public partial class SelectLocationPage : ContentPage
{
    public SelectLocationPage(SelectLocationPageViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;       
    }
}