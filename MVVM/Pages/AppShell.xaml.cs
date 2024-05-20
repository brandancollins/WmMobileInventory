using System.Diagnostics;
using WmMobileInventory.Services;

namespace WmMobileInventory.MVVM.Pages;

public partial class AppShell : Shell
{
    private readonly IAuthService _authService;
    private readonly IInventoryService _inventoryService;

    public AppShell(IAuthService authService, IInventoryService inventoryService)
    {
        InitializeComponent();
        _authService = authService;
        _inventoryService = inventoryService;

        // Handle navigation to LoginPage and force logout if needed
        Navigated += async (sender, e) =>
        {
            Debug.WriteLine($"--- A navigation was performed: {e.Source}, " +
            $"from {e.Previous?.Location.ToString()} to {e.Current?.Location.ToString()}");

            if (e.Source == ShellNavigationSource.ShellSectionChanged && e.Current.Location.ToString() == "//loginPage/Home")
            {
                if (_authService.IsLoggedIn)
                {
                    await _authService.LogoutAsync();
                }
            }
        };

        // Register routes for navigation
        RegisterRoutes();
        // Setup event handler for navigation
        Navigating += OnNavigating;
    }

    private void RegisterRoutes()
    {
        Routing.RegisterRoute(nameof(SelectDeptPage), typeof(SelectDeptPage));
        Routing.RegisterRoute(nameof(SelectLocationPage), typeof(SelectLocationPage));
        Routing.RegisterRoute(nameof(SelectRoomPage), typeof(SelectRoomPage));
        Routing.RegisterRoute(nameof(ScanAssetPage), typeof(ScanAssetPage));
    }

    private async void OnNavigating(object? sender, ShellNavigatingEventArgs e)
    {
        var currentRoute = e.Target.Location.OriginalString;

        // Check for required information before navigating to specific pages
        if (currentRoute.Contains("selectLocationPage") && string.IsNullOrEmpty(_inventoryService.CurrentDepartment))
        {
            e.Cancel();
            await this.GoToAsync("//selectDeptPage");
        }
        else if (currentRoute.Contains("selectRoomPage") && (string.IsNullOrEmpty(_inventoryService.CurrentDepartment) || string.IsNullOrEmpty(_inventoryService.CurrentLocation)))
        {
            e.Cancel();
            if (string.IsNullOrEmpty(_inventoryService.CurrentDepartment))
            {
                await this.GoToAsync("//selectDeptPage");
            }
            else
            {
                await this.GoToAsync("//selectLocationPage");
            }            
        }
        else if (currentRoute.Contains("scanAssetPage") && (string.IsNullOrEmpty(_inventoryService.CurrentDepartment) || string.IsNullOrEmpty(_inventoryService.CurrentLocation) || string.IsNullOrEmpty(_inventoryService.CurrentRoom)))
        {
            e.Cancel();
            if (string.IsNullOrEmpty(_inventoryService.CurrentDepartment))
            {
                await this.GoToAsync("//selectDeptPage");
            }
            else if (string.IsNullOrEmpty(_inventoryService.CurrentLocation))
            {
                await this.GoToAsync("//selectLocationPage");
            }
            else
            {
                await this.GoToAsync("//selectRoomPage");
            }
        }
    }


}
