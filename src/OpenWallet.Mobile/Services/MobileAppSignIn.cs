using OpenWallet.Mobile.Core.Auth;
using OpenWallet.Shared.Constants;
using OpenWallet.Shared.DTOs;
using OpenWallet.UI.Services;

namespace OpenWallet.Mobile.Services;

/// <summary>Signing in from the app's sign-in page: with a password in the app, or in the browser (passkeys).</summary>
public sealed class MobileAppSignIn(SessionManager session) : IAppSignIn
{
    public string? ServerAddress => session.ServerAddress;

    public Task<ServerInfoDto> SetServerAsync(string address) => session.SetServerAsync(address);

    public Task<LoginResultDto> SignInAsync(AppLoginDto dto) => session.SignInAsync(dto);

    /// <summary>
    /// Opens the server's mobile-login page in the browser (a Custom Tab). After signing in there and
    /// confirming, the server sends the browser to openwallet://auth with a one-time code, which only
    /// becomes tokens together with the PKCE verifier kept here.
    /// </summary>
    public async Task SignInWithBrowserAsync()
    {
        if (session.ServerAddress == null)
            throw new AppSignInException("Choose your server first.");

        (string verifier, string challenge) = Pkce.Create();
        Uri start = new(new Uri(session.ServerAddress), $"{MobileApp.StartPath}?challenge={challenge}");
        WebAuthenticatorResult result = await WebAuthenticator.Default.AuthenticateAsync(new WebAuthenticatorOptions
        {
            Url = start,
            CallbackUrl = new Uri(MobileApp.CallbackUri),
            PrefersEphemeralWebBrowserSession = false
        });

        if (!result.Properties.TryGetValue("code", out string? code) || string.IsNullOrEmpty(code))
            throw new AppSignInException("The browser didn't finish signing in. Try again.");

        await session.SignInWithCodeAsync(code, verifier);
    }
}
