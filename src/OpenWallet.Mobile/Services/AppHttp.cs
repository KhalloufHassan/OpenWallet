using OpenWallet.Mobile.Core.Auth;

namespace OpenWallet.Mobile.Services;

/// <summary>
/// Builds the app's HttpClients around one platform handler, so they share connections:
/// <list type="bullet">
/// <item>to the server, for the pages and attachments: the token, then the chosen server;</item>
/// <item>plain, for signing in and checking a server.</item>
/// </list>
/// </summary>
public sealed class AppHttp(IServiceProvider services)
{
    readonly HttpMessageHandler platform = new HttpClientHandler();

    public HttpClient CreatePlain(string address) => new(platform, disposeHandler: false) { BaseAddress = new Uri(address) };

    public HttpClient CreateToServer()
    {
        SessionManager session = services.GetRequiredService<SessionManager>();
        BearerTokenHandler handler = new(session)
        {
            InnerHandler = new ServerAddressHandler(session) { InnerHandler = new NonDisposingHandler(platform) }
        };
        return new HttpClient(handler, disposeHandler: true) { BaseAddress = ServerAddressHandler.Placeholder };
    }

    /// <summary>Keeps the shared platform handler alive when a client and its handlers are disposed.</summary>
    sealed class NonDisposingHandler(HttpMessageHandler inner) : DelegatingHandler(inner)
    {
        protected override void Dispose(bool disposing)
        {
        }
    }
}
