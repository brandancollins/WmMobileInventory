using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using WmMobileInventory.MVVM.Pages;
using WmMobileInventory.MVVM.ViewModels;
using WmMobileInventory.Services;

namespace WmMobileInventory
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                    fonts.AddFont("MaterialSymbolsRounded.ttf", "MaterialSymbols");
                });

#if DEBUG
    		builder.Logging.AddDebug();
#endif
            // pages
            builder.Services.AddTransient<AppShell>();
            builder.Services.AddTransient<LoginPage>();
            builder.Services.AddTransient<SelectDeptPage>();
            builder.Services.AddTransient<SelectLocationPage>();
            builder.Services.AddTransient<SelectRoomPage>();
            builder.Services.AddTransient<ScanAssetPage>();
            builder.Services.AddTransient<InventoriedReviewPage>();
            builder.Services.AddTransient<NotInventoriedReviewPage>();
            builder.Services.AddTransient<SummaryReviewPage>();

            // view models
            builder.Services.AddTransient<LoginPageViewModel>();
            builder.Services.AddTransient<SelectDeptPageViewModel>();
            builder.Services.AddTransient<SelectLocationPageViewModel>();
            builder.Services.AddTransient<SelectRoomPageViewModel>();
            builder.Services.AddTransient<ScanAssetPageViewModel>();
            builder.Services.AddTransient<InventoriedReviewPageViewModel>();
            builder.Services.AddTransient<NotInventoriedReviewPageViewModel>();
            builder.Services.AddTransient<SummaryReviewPageViewModel>();

            // services
            builder.Services.AddSingleton<ConfigurationService>();
            builder.Services.AddSingleton<DatabaseService>(serviceProvider =>
            {
                var configService = serviceProvider.GetRequiredService<ConfigurationService>();
                var configuration = configService.GetConfigurationAsync().Result; // Load settings synchronously here for simplicity
                  return new DatabaseService(configuration.AppSettings.ServiceUrl);
            });
            builder.Services.AddSingleton<IAuthService, AuthService>();
            builder.Services.AddSingleton<IInventoryService, InventoryService>();

            return builder.Build();
        }
    }
}
