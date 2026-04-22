using Microsoft.JSInterop;

namespace OpenWallet.Client.Services;

public class PrivacyService(IJSRuntime js)
{
    const string Key = "ow_privacy_masked";

    public bool IsMasked { get; private set; }
    public event Action? OnChanged;

    public async Task InitAsync()
    {
        string? value = await js.InvokeAsync<string?>("localStorage.getItem", Key);
        IsMasked = value == "true";
    }

    public async Task ToggleAsync()
    {
        IsMasked = !IsMasked;
        await js.InvokeVoidAsync("localStorage.setItem", Key, IsMasked ? "true" : "false");
        OnChanged?.Invoke();
    }
}
