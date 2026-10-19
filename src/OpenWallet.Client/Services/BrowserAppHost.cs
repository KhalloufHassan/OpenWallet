using Microsoft.JSInterop;
using OpenWallet.Shared.DTOs;
using OpenWallet.UI.Services;

namespace OpenWallet.Client.Services;

/// <summary>The web app: location and downloads through the browser, which sends the session cookie itself.</summary>
public class BrowserAppHost(IJSRuntime js) : IAppHost
{
    public bool IsNativeApp => false;

    public async Task<GeoCoords> GetCurrentPositionAsync() =>
        await js.InvokeAsync<GeoCoords>("getCurrentPosition");

    public async Task OpenAttachmentAsync(int recordId, AttachmentDto attachment) =>
        await js.InvokeVoidAsync("owFiles.download", $"api/records/{recordId}/attachments/{attachment.Id}", attachment.FileName);
}
