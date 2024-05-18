using System.Diagnostics;
using WmMobileInventory.MVVM.ViewModels;
using WmMobileInventory.Services;

namespace WmMobileInventory.MVVM.Pages;

public partial class AppShell : Shell {

    private readonly IAuthService _authService;

    public AppShell(IAuthService authService)
    {
        InitializeComponent();
        _authService = authService;

        // Handle navigation to LoginPage and force logout if needed
        Navigated += async (sender, e) =>
        {
            if (e.Source == ShellNavigationSource.ShellSectionChanged && e.Current.Location.ToString() == "//loginPage/Home")
            {
                if (_authService.IsLoggedIn)
                {
                    await _authService.LogoutAsync();
                }
            }
        };
    }

    //public AppShell(AppShellViewModel vm)
    //{
    //    InitializeComponent();
    //    BindingContext = vm;

    //    // Navigate to LoginPage.
    //    GoToAsync("//loginPage");
    //}

    //protected override void OnNavigated(ShellNavigatedEventArgs args)
    //{
    //    base.OnNavigated(args);

    //    Debug.WriteLine($"--- A navigation was performed: {args.Source}, " +
    //        $"from {args.Previous?.Location.ToString()} to {args.Current?.Location.ToString()}");

    //    if (args.Current != null)
    //    {
    //        if (args.Current?.Location != null && args.Current?.Location.ToString() == "//loginPage/Home") 
    //        {
    //            LoginPageViewModel lvm = 
    //        }
    //    }

    //}
}