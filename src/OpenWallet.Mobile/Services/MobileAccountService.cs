using Microsoft.AspNetCore.Components;
using OpenWallet.Mobile.Core.Auth;
using OpenWallet.Mobile.Core.Lock;
using OpenWallet.UI.Services;

namespace OpenWallet.Mobile.Services;

/// <summary>Sign-in actions in the app: the app's own sign-in page, and the server's pages in the browser for passkeys.</summary>
public sealed class MobileAccountService(NavigationManager nav, SessionManager session, AppLockState lockState) : IAccountService
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

    /// <summary>The app locks behind the device's own unlock instead of signing out.</summary>
    public Task LockAsync()
    {
        lockState.Lock();
        return Task.CompletedTask;
    }

    /// <summary>Passkeys are added in the browser, where they belong to the server's address.</summary>
    public void OpenSecuritySettings()
    {
        if (session.ServerAddress is string server)
            _ = Browser.Default.OpenAsync(new Uri(new Uri(server), "security"), BrowserLaunchMode.SystemPreferred);
    }
}
