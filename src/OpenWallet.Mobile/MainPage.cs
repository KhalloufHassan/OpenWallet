using Microsoft.AspNetCore.Components.WebView.Maui;
using OpenWallet.Mobile.Services;

namespace OpenWallet.Mobile;

/// <summary>The app's only page: a web view showing OpenWallet's pages (OpenWallet.UI).</summary>
public sealed class MainPage : ContentPage
{
    readonly BlazorWebView webView;

    public MainPage(WebViewRequests requests)
    {
        webView = new BlazorWebView { HostPage = "wwwroot/index.html" };
        webView.RootComponents.Add(new RootComponent { Selector = "#app", ComponentType = typeof(OpenWallet.UI.Routes) });
        webView.WebResourceRequested += requests.OnWebResourceRequested;
        BackgroundColor = Color.FromArgb("#0D1117");
        Content = webView;
    }

    /// <summary>Android's back button goes back in the app, and only leaves it from the first page.</summary>
    protected override bool OnBackButtonPressed()
    {
#if ANDROID
        if (webView.Handler?.PlatformView is Android.Webkit.WebView web && web.CanGoBack())
        {
            web.GoBack();
            return true;
        }
#endif
        return base.OnBackButtonPressed();
    }
}
