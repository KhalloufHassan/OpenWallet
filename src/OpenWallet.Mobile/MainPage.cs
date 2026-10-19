using Microsoft.AspNetCore.Components.WebView.Maui;
using OpenWallet.Mobile.Core.Auth;
using OpenWallet.Mobile.Core.Lock;
using OpenWallet.Mobile.Services;

namespace OpenWallet.Mobile;

/// <summary>
/// The app's only page: a web view showing OpenWallet's pages (OpenWallet.UI), under a lock screen
/// that covers it whenever the app is locked or off screen.
/// </summary>
public sealed class MainPage : ContentPage
{
    readonly BlazorWebView webView;
    readonly Grid lockScreen;
    readonly AppLockState lockState;
    readonly SessionManager session;
    readonly DeviceUnlock deviceUnlock;
    bool started;
    bool prompting;

    public MainPage(WebViewRequests requests, AppLockState lockState, SessionManager session, DeviceUnlock deviceUnlock)
    {
        this.lockState = lockState;
        this.session = session;
        this.deviceUnlock = deviceUnlock;

        webView = new BlazorWebView { HostPage = "wwwroot/index.html" };
        webView.RootComponents.Add(new RootComponent { Selector = "#app", ComponentType = typeof(OpenWallet.UI.Routes) });
        webView.WebResourceRequested += requests.OnWebResourceRequested;

        lockScreen = CreateLockScreen();
        BackgroundColor = Color.FromArgb("#0D1117");
        Content = new Grid { Children = { webView, lockScreen } };

        lockState.Changed += () => MainThread.BeginInvokeOnMainThread(OnLockChanged);
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (started) return;
        started = true;

        await session.InitializeAsync();
        if (!session.IsSignedIn)
            lockState.Unlock();
        else
            await PromptAsync();
    }

    /// <summary>Android's back button goes back in the app, and only leaves it from the first page.</summary>
    protected override bool OnBackButtonPressed()
    {
#if ANDROID
        if (!lockState.IsHidden && webView.Handler?.PlatformView is Android.Webkit.WebView web && web.CanGoBack())
        {
            web.GoBack();
            return true;
        }
#endif
        return base.OnBackButtonPressed();
    }

    async void OnLockChanged()
    {
        lockScreen.IsVisible = lockState.IsHidden;
        if (started && lockState.NeedsUnlock)
            await PromptAsync();
    }

    async Task PromptAsync()
    {
        if (prompting || !lockState.NeedsUnlock) return;
        prompting = true;
        try
        {
            DeviceUnlockResult result = await deviceUnlock.UnlockAsync();
            if (result is DeviceUnlockResult.Unlocked or DeviceUnlockResult.Unavailable)
                lockState.Unlock();
        }
        finally
        {
            prompting = false;
        }
    }

    Grid CreateLockScreen()
    {
        Button unlock = new()
        {
            Text = "Unlock",
            BackgroundColor = Color.FromArgb("#0D6EFD"),
            TextColor = Colors.White,
            CornerRadius = 8,
            WidthRequest = 200
        };
        unlock.Clicked += async (_, _) => await PromptAsync();

        return new Grid
        {
            BackgroundColor = Color.FromArgb("#0D1117"),
            Children =
            {
                new VerticalStackLayout
                {
                    Spacing = 16,
                    VerticalOptions = LayoutOptions.Center,
                    HorizontalOptions = LayoutOptions.Center,
                    Children =
                    {
                        new Label { Text = "OpenWallet", FontSize = 26, FontAttributes = FontAttributes.Bold, TextColor = Colors.White, HorizontalTextAlignment = TextAlignment.Center },
                        new Label { Text = "Locked", FontSize = 15, TextColor = Color.FromArgb("#ADB5BD"), HorizontalTextAlignment = TextAlignment.Center },
                        unlock
                    }
                }
            }
        };
    }
}
