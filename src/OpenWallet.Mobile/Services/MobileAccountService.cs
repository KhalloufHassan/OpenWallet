using Microsoft.AspNetCore.Components;
using OpenWallet.Mobile.Core.Auth;
using OpenWallet.UI.Services;

namespace OpenWallet.Mobile.Services;

/// <summary>Sign-in actions in the app: the app's own sign-in page, and the server's pages in the browser for passkeys.</summary>
public sealed class MobileAccountService(NavigationManager nav, SessionManager session) : IAccountService
{
    public async Task<bool> HasSessionAsync()
    {
        await session.InitializeAsync();
        return session.IsSignedIn;
    }

    public void RedirectToLogin()
    {
        string returnUrl = nav.ToBaseRelativePath(nav.Uri);
        nav.NavigateTo(string.IsNullOrEmpty(returnUrl) ? "app-login" : $"app-login?returnUrl={Uri.EscapeDataString(returnUrl)}", replace: true);
    }

    public async Task SignOutAsync()
    {
        await session.SignOutAsync();
        nav.NavigateTo("app-login", replace: true);
    }

    public async Task LockAsync()
    {
        await session.SignOutAsync();
        nav.NavigateTo("app-login?locked=true", replace: true);
    }

    /// <summary>Passkeys are added in the browser, where they belong to the server's address.</summary>
    public void OpenSecuritySettings()
    {
        if (session.ServerAddress is string server)
            _ = Browser.Default.OpenAsync(new Uri(new Uri(server), "security"), BrowserLaunchMode.SystemPreferred);
    }
}
