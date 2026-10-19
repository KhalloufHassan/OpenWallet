using Microsoft.JSInterop;

namespace OpenWallet.UI.Services;

public class LockService(IJSRuntime js, IAccountService account)
{
    public async Task InitAsync(DotNetObjectReference<LockService> selfRef)
    {
        await js.InvokeVoidAsync("owAuth.startInactivityTimer", selfRef, 5);
    }

    [JSInvokable]
    public Task OnInactivityTimeout() => account.LockAsync();
}
