using Microsoft.AspNetCore.Components;
using OpenWallet.UI.Services;

namespace OpenWallet.Client.Services;

/// <summary>The web app signs in on its login page and keeps the session in a cookie.</summary>
public class WebAccountService(NavigationManager nav, ApiClient api) : IAccountService
{
    public Task<bool> HasSessionAsync() => Task.FromResult(true);

    public void RedirectToLogin()
    {
        string returnUrl = nav.ToBaseRelativePath(nav.Uri);
        nav.NavigateTo(string.IsNullOrEmpty(returnUrl) ? "/login" : $"/login?returnUrl={Uri.EscapeDataString(returnUrl)}");
    }

    public async Task SignOutAsync()
    {
        await api.LogoutAsync();
        nav.NavigateTo("/login", forceLoad: true);
    }

    public async Task LockAsync()
    {
        await api.LogoutAsync();
        nav.NavigateTo("/login?locked=true", forceLoad: true);
    }

    public void OpenSecuritySettings() => nav.NavigateTo("/security");
}
