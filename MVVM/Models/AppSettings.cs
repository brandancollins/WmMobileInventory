namespace WmMobileInventory.MVVM.Models
{
    public class Configuration
    {
        public AppSettings AppSettings { get; set; }
        public AzureAdSettings AzureAD { get; set; }
        public MSGraphApiSettings MSGraphApi { get; set; }
    }

    public class AppSettings
    {
        public string ServiceUrl { get; set; }
    }

    public class AzureAdSettings
    {
        public string Authority { get; set; }
        public string TenantId { get; set; }
        public string ClientId { get; set; }
        public string RedirectURI { get; set; }
        public string ApiScopes { get; set; }
        public string AndroidRedirectUri { get; set; }
        public string iOSRedirectUri { get; set; }
        public string CacheFileName { get; set; }
        public string CacheDir { get; set; }
    }

    public class MSGraphApiSettings
    {
        public string MSGraphBaseUrl { get; set; }
        public string Scopes { get; set; }
    }


}
