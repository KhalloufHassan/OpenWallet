using Microsoft.Extensions.Logging;
using OpenWallet.Mobile.Core.Auth;
using OpenWallet.Mobile.Core.WebView;
using OpenWallet.Mobile.Services;
using OpenWallet.UI.Services;

namespace OpenWallet.Mobile;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        MauiAppBuilder builder = MauiApp.CreateBuilder();
        builder.UseMauiApp<App>();

        builder.Services.AddMauiBlazorWebView();
#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();
#endif

        builder.Services.AddSingleton(TimeProvider.System);
        builder.Services.AddSingleton<AppHttp>();
        builder.Services.AddSingleton<ISessionStorage, SecureSessionStorage>();
        builder.Services.AddSingleton(services => new SessionManager(
            services.GetRequiredService<ISessionStorage>(),
            address => services.GetRequiredService<AppHttp>().CreatePlain(address),
            services.GetRequiredService<TimeProvider>(),
            AppInfo.Current.VersionString));
        builder.Services.AddSingleton(services => new WebViewFiles(services.GetRequiredService<AppHttp>().CreateToServer));
        builder.Services.AddSingleton<WebViewRequests>();
        builder.Services.AddSingleton<MainPage>();

        builder.Services.AddScoped(services => new ApiClient(services.GetRequiredService<AppHttp>().CreateToServer()));
        builder.Services.AddScoped<IAccountService, MobileAccountService>();
        builder.Services.AddScoped<IAppHost, MobileAppHost>();
        builder.Services.AddSingleton<IAppSignIn, MobileAppSignIn>();
        builder.Services.AddScoped<PrivacyService>();
        builder.Services.AddScoped<LockService>();

        return builder.Build();
    }
}
