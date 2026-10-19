using Android.App;
using Android.Content;
using Android.Content.PM;
using Microsoft.Maui.Authentication;

namespace OpenWallet.Mobile;

/// <summary>
/// Receives openwallet://auth?code=… from the browser at the end of the passkey sign-in (see
/// MobileAppSignIn) and hands it back to the waiting WebAuthenticator. The scheme and host must
/// match MobileApp.CallbackUri.
/// </summary>
[Activity(NoHistory = true, LaunchMode = LaunchMode.SingleTop, Exported = true)]
[IntentFilter(
    [Intent.ActionView],
    Categories = [Intent.CategoryDefault, Intent.CategoryBrowsable],
    DataScheme = "openwallet",
    DataHost = "auth")]
public class SignInCallbackActivity : WebAuthenticatorCallbackActivity
{
}
