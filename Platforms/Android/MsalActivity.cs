using Android.App;
using Android.Content;
using Microsoft.Identity.Client;

namespace WmMobileInventory.Platforms.Android.Resources
{
    [Activity(Exported = true)]
    [IntentFilter(new[] { Intent.ActionView },
         Categories = new[] { Intent.CategoryBrowsable, Intent.CategoryDefault },
         DataHost = "auth",
         DataScheme = "msalf6e51cba-8a52-4dd7-8110-b3fb45849af7")]
    public class MsalActivity : BrowserTabActivity
    {
    }
}
