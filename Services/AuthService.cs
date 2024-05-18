using MAUI.MSALClient;
using Microsoft.Identity.Client;
using WmAssetWebServiceClientNet.Models;
using WmMobileInventory.MVVM.Models;
using static Android.Media.MediaDrm;

namespace WmMobileInventory.Services;

public interface IAuthService
{
    bool IsLoggedIn { get; }
    CurrentUser CurrentUser { get; }
    string LoginMessage { get; }
    Task LoginAsync();
    Task LogoutAsync();
    Task TrackLoginStatusAsync();
}

public class AuthService : IAuthService
{
    private readonly DatabaseService _databaseService;
    private Timer? _logoutTimer;

    public CurrentUser CurrentUser { get; }

    public bool IsLoggedIn { get; private set; }
    public string LoginMessage { get; private set; } = string.Empty;

    public AuthService(DatabaseService databaseService)
    {
        _databaseService = databaseService;
        IsLoggedIn = false;
        CurrentUser = new CurrentUser();
    }

    public async Task LoginAsync()
    {
        PublicClientSingleton.Instance.UseEmbedded = false;
        try
        {
            await PublicClientSingleton.Instance.AcquireTokenSilentAsync();
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
                }
            }
            catch (Exception dsEx)
            {
                Console.WriteLine(dsEx.Message);
                Console.WriteLine(dsEx.InnerException.Message);

            }
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

    private void StopLogoutTimer()
    {
        _logoutTimer?.Dispose();
        _logoutTimer = null;
    }

    public async Task TrackLoginStatusAsync()
    {
        while (IsLoggedIn)
        {
            await Task.Delay(1000);
            // Add any additional tracking logic here
        }
    }
}
