using WmMobileInventory.MVVM.Pages;
using WmMobileInventory.Services;

namespace WmMobileInventory
{
    public partial class App : Application
    {
        private readonly IAuthService _authService;

        public App(IAuthService authService, IServiceProvider serviceProvider)
        {
            InitializeComponent();

            _authService = authService;

            // Resolve the LoginPage using the service provider
            MainPage = serviceProvider.GetRequiredService<AppShell>();

            // Force navigation to LoginPage on startup
            Shell.Current.GoToAsync("//loginPage/Home");
        }

        //public App(IServiceProvider serviceProvider)
        //{
        //    InitializeComponent();

        //    MainPage = serviceProvider.GetRequiredService<AppShell>();
        //}
    }
}
