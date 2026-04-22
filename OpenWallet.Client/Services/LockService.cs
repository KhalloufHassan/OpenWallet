using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace OpenWallet.Client.Services;

public class LockService(IJSRuntime js, ApiClient api, NavigationManager nav)
{
    public async Task InitAsync(DotNetObjectReference<LockService> selfRef)
    {
        await js.InvokeVoidAsync("owAuth.startInactivityTimer", selfRef, 5);
    }

    [JSInvokable]
    public async Task OnInactivityTimeout()
    {
        await api.LogoutAsync();
        nav.NavigateTo("/login?locked=true", forceLoad: true);
    }
}
