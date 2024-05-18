using System.Reflection;
using System.Text.Json;
using WmMobileInventory.MVVM.Models;

namespace WmMobileInventory.Services;

public class ConfigurationService
{
    private const string ResourcePath = "WmMobileInventory.appsettings.json";

    public async Task<Configuration> GetConfigurationAsync()
    {
        var assembly = Assembly.GetExecutingAssembly();
        using var stream = assembly.GetManifestResourceStream(ResourcePath);
        if (stream != null)
        {
            using var reader = new StreamReader(stream);
            var json = await reader.ReadToEndAsync();
            return JsonSerializer.Deserialize<Configuration>(json);
        }
        return new Configuration(); // Return default settings if file not found
    }
}
