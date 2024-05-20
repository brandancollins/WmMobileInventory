using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WmMobileInventory.Services;

namespace WmMobileInventory.MVVM.ViewModels;

public partial class LoginPageViewModel : ObservableObject
{
    private readonly IAuthService _authService;

    [ObservableProperty]
    public string loginMessage;

    public LoginPageViewModel(IAuthService authService)
    {
        _authService = authService;
        LoginMessage = _authService.LoginMessage;
    }

    [RelayCommand]
    private async Task LoginAsync()
    {
        await _authService.LoginAsync();
        LoginMessage = _authService.LoginMessage;
        if (_authService.IsLoggedIn)
        {
            LoginMessage = string.Empty;
            await Shell.Current.GoToAsync("//inventory/selectDeptPage"); // Navigate to a different page after login
        }       
    }
}
