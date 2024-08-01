using System.Diagnostics;
using WmMobileInventory.MVVM.ViewModels;
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

            if (e.Source == ShellNavigationSource.ShellItemChanged && e.Current.Location.ToString() == "//loginPage/Home")
            {
                if (_authService.IsLoggedIn)
                {
                    await _authService.LogoutAsync();
                }
            }
            if ((e.Source == ShellNavigationSource.ShellSectionChanged || e.Source == ShellNavigationSource.ShellItemChanged ) && 
                 e.Current.Location.ToString() == "//inventory/selectDeptPage")
            {
                if (this.CurrentPage.BindingContext is SelectDeptPageViewModel viewModel)
                {
                    viewModel.OnSelectedItemChanged();
                }
            }
            if (e.Source == ShellNavigationSource.ShellSectionChanged && e.Current.Location.ToString() == "//inventory/selectRoomPage")
            {
                // Assuming your SelectRoomPage's BindingContext is set to SelectRoomPageViewModel
                if (this.CurrentPage.BindingContext is SelectRoomPageViewModel viewModel)
                {
                    viewModel.RefreshRooms();
                }
            }
            if ((e.Source == ShellNavigationSource.ShellSectionChanged && e.Current.Location.ToString() == "//inventory/scanAssetPage") ||
                (e.Source == ShellNavigationSource.PopToRoot && e.Current.Location.ToString() == "//inventory/scanAssetPage"))
            {
                if (this.CurrentPage.BindingContext is ScanAssetPageViewModel viewModel)
                {
                    viewModel.SetTitleText();
                    viewModel.RefreshCurrentAsset();
                }
            }
            if ((e.Source == ShellNavigationSource.PopToRoot && e.Current.Location.ToString() == "//inventoriedReviewPage/notInventoried") ||
                (e.Source == ShellNavigationSource.ShellSectionChanged && e.Current.Location.ToString() == "//inventoriedReviewPage/notInventoried"))
            {
                if (this.CurrentPage.BindingContext is NotInventoriedReviewPageViewModel viewModel)
                {
                    await viewModel.RefreshNotLocatedAssets();
                }
            }

            if ((e.Source == ShellNavigationSource.PopToRoot && e.Current.Location.ToString() == "//inventoriedReviewPage/inventoried") ||
                (e.Source == ShellNavigationSource.ShellSectionChanged && e.Current.Location.ToString() == "//inventoriedReviewPage/inventoried"))
            {
                if (this.CurrentPage.BindingContext is InventoriedReviewPageViewModel viewModel)
                {
                    await viewModel.RefreshLocatedAssets();
                }
            }

            if ((e.Source == ShellNavigationSource.ShellSectionChanged && e.Current.Location.ToString() == "//inventoriedReviewPage/summary") ||
                (e.Source == ShellNavigationSource.ShellItemChanged && e.Current.Location.ToString() == "//inventoriedReviewPage/summary"))
            {
                if (this.CurrentPage.BindingContext is SummaryReviewPageViewModel viewModel)
                {
                    await viewModel.RefreshSummary();
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
