using WmAssetWebServiceClientNet;

namespace WmMobileInventory.Services
{
    public class DatabaseService
    {
        public AssetDataRepository AssetDataRepository { get; }

        public DatabaseService(string serviceUrl)
        {
            AssetDataRepository = new AssetDataRepository(new HttpClient { BaseAddress = new Uri(serviceUrl) });
        }
    }
}
