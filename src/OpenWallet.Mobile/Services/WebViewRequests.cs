using OpenWallet.Mobile.Core.WebView;

namespace OpenWallet.Mobile.Services;

/// <summary>
/// Answers the web view's API requests (attachment images) through <see cref="WebViewFiles"/>,
/// using .NET MAUI's web request interception.
/// </summary>
public sealed class WebViewRequests(WebViewFiles files)
{
    public void OnWebResourceRequested(object? sender, WebViewWebResourceRequestedEventArgs e)
    {
        if (!WebViewFiles.Handles(e.Method, e.Uri))
            return;

        e.Handled = true;
        Dictionary<string, string> headers = new()
        {
            ["Content-Type"] = "application/octet-stream",
            ["Cache-Control"] = "no-store"
        };

        e.SetResponse(200, "OK", headers, FetchAsync(e.Uri));
    }

    async Task<Stream?> FetchAsync(Uri uri) => await files.FetchAsync(uri, CancellationToken.None);
}
