using MAUI.MSALClient;
using Microsoft.Identity.Client;
using WmAssetWebServiceClientNet.Models;
using WmMobileInventory.MVVM.Models;

namespace WmMobileInventory.Services;

public interface IAuthService
{
    bool IsLoggedIn { get; }
    CurrentUser CurrentUser { get; }
    string LoginMessage { get; }
    Task LoginAsync();
    Task LogoutAsync();
    //Task TrackLoginStatusAsync();
}

public class AuthService : IAuthService
{
    private readonly DatabaseService _databaseService;
    private readonly ConfigurationService  _configurationService;
    private string _accessToken;
    private Timer? _logoutTimer;
    private Timer? _tokenStatusTimer;

    public CurrentUser CurrentUser { get; }

    public bool IsLoggedIn { get; private set; }
    public string LoginMessage { get; private set; } = string.Empty;

    public AuthService(DatabaseService databaseService, ConfigurationService configurationService)
    {
        _databaseService = databaseService;
        IsLoggedIn = false;
        CurrentUser = new CurrentUser();
        _configurationService = configurationService;
    }

    public async Task LoginAsync()
    {
        var configuration = await _configurationService.GetConfigurationAsync();
        string[] _scopes = configuration.AzureAD.ApiScopes.Split(',');
        PublicClientSingleton.Instance.UseEmbedded = false;
        try
        {
            _accessToken = await PublicClientSingleton.Instance.AcquireTokenSilentAsync(_scopes);
            if (!string.IsNullOrEmpty(_accessToken))
            {
                _databaseService.AssetDataRepository.SetAccessToken(_accessToken);
            }
        }
        catch (MsalClientException ex) when (ex.ErrorCode == MsalError.AuthenticationCanceledError)
        {
            LoginMessage = "User cancelled sign in.";
            return;
        }
        catch (MsalServiceException ex) when (ex.Message.Contains("AADSTS65004"))
        {
            LoginMessage = "User did not consent to app requirements.";
            return;
        }

        var graphUser = await PublicClientSingleton.Instance.MSGraphHelper.GetMeAsync();

        if (graphUser.DisplayName != null)
        {
            string displayName = graphUser.DisplayName;
            // Now make aure the authenticated user has appliction access.
            try
            {
                IEnumerable<User> users = await _databaseService.AssetDataRepository.GetUsers();
                // find the user in the IEnumerable<User> users by username.
                User? dbuser = users.FirstOrDefault(u => u.UserName.Contains(displayName));
                if (dbuser != null)
                {
                    IsLoggedIn = true;
                    CurrentUser.Departments = dbuser.Departments;
                    CurrentUser.AllDepartments = dbuser.AllDepartments;
                    CurrentUser.Username = displayName;
                    StartLogoutTimer();
                    SetupTokenStatusTimer(PublicClientSingleton.Instance.MSALClientHelper.AuthResult.ExpiresOn);
                }
            }
            catch (Exception dsEx)
            {
                Console.WriteLine(dsEx.Message);
                Console.WriteLine(dsEx.InnerException.Message);

            }
        }
    }

    private async Task LoginRefresh()
    {
        var configuration = await _configurationService.GetConfigurationAsync();
        string[] _scopes = configuration.AzureAD.ApiScopes.Split(',');
        PublicClientSingleton.Instance.UseEmbedded = false;
        try
        {
            _accessToken = await PublicClientSingleton.Instance.AcquireTokenSilentAsync(_scopes);
            if (!string.IsNullOrEmpty(_accessToken))
            {
                _databaseService.AssetDataRepository.SetAccessToken(_accessToken);
            }
            else
            {
                await LogoutAsync();
            }
        }
        catch (MsalClientException ex) when (ex.ErrorCode == MsalError.AuthenticationCanceledError)
        {
            LoginMessage = "User cancelled sign in.";
            return;
        }
        catch (MsalServiceException ex) when (ex.Message.Contains("AADSTS65004"))
        {
            LoginMessage = "User did not consent to app requirements.";
            return;
        }
    }

    public async Task LogoutAsync()
    {
        await PublicClientSingleton.Instance.SignOutAsync();
        IsLoggedIn = false;
        StopLogoutTimer();
        await Shell.Current.GoToAsync("//loginPage/Home");
    }

    private void StartLogoutTimer()
    {
        _logoutTimer = new Timer(async _ =>
        {
            if (IsLoggedIn)
            {
                await LogoutAsync();
            }
        }, null, TimeSpan.FromHours(4), Timeout.InfiniteTimeSpan);
    }

    private void SetupTokenStatusTimer(DateTimeOffset expiresOn)
    {
        TimeSpan interval = expiresOn - DateTimeOffset.Now;
        // Convert the interval to milliseconds
        int dueTime = (int)interval.TotalMilliseconds;

        // Create the timer and pass the interval and the callback method
        _tokenStatusTimer = new Timer(TokenStatusTimerCallback, null, dueTime, Timeout.Infinite);
    }

    private async void TokenStatusTimerCallback(object? state)
    {
        // Token has expired, acquire a new one
        await LoginRefresh();
        // Dispose of the timer if it's no longer needed
        SetupTokenStatusTimer(PublicClientSingleton.Instance.MSALClientHelper.AuthResult.ExpiresOn);
    }


    private void StopLogoutTimer()
    {
        _logoutTimer?.Dispose();
        _logoutTimer = null;
        _tokenStatusTimer?.Dispose();
        _tokenStatusTimer = null;
    }

    //public async Task TrackLoginStatusAsync()
    //{
    //    while (IsLoggedIn)
    //    {
    //        await Task.Delay(1000);
    //        // Add any additional tracking logic here
    //    }
    //}
}
