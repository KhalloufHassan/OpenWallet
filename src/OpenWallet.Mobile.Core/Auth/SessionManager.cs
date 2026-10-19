using System.Net.Http.Headers;
using System.Text.Json;
using OpenWallet.Shared.Constants;
using OpenWallet.Shared.DTOs;
using OpenWallet.UI.Services;

namespace OpenWallet.Mobile.Core.Auth;

/// <summary>
/// The app's server address and sign-in: signs in, keeps the tokens fresh (one refresh at a time,
/// however many requests need it) and signs out when the server no longer accepts them.
/// </summary>
/// <param name="createClient">An HttpClient for the given server address, without the app's own handlers.</param>
public sealed class SessionManager(ISessionStorage storage, Func<string, HttpClient> createClient, TimeProvider timeProvider, string appVersion)
{
    /// <summary>Tokens are refreshed this long before they expire, so requests don't race the expiry.</summary>
    static readonly TimeSpan RefreshMargin = TimeSpan.FromMinutes(2);

    readonly SemaphoreSlim refreshLock = new(1, 1);
    bool initialized;

    public string? ServerAddress { get; private set; }

    public AppSession? Session { get; private set; }

    public bool IsSignedIn => Session != null;

    /// <summary>Signed in or out (including when the server rejected the session).</summary>
    public event Action? Changed;

    public async Task InitializeAsync()
    {
        if (initialized) return;

        ServerAddress = await storage.GetServerAddressAsync();
        Session = ServerAddress == null ? null : await storage.LoadSessionAsync();
        initialized = true;
    }

    /// <summary>Checks the server and this app work together, then remembers it. Changing server signs out.</summary>
    public async Task<ServerInfoDto> SetServerAsync(string input)
    {
        string address = Auth.ServerAddress.Normalize(input);
        ServerInfoDto? info;
        try
        {
            using HttpClient http = createClient(address);
            info = await new ApiClient(http).GetServerInfoAsync();
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or JsonException or NotSupportedException)
        {
            throw new AppSignInException("No OpenWallet server answered at that address. Check it, and that this phone can reach it.");
        }

        if (info?.Name != MobileApp.ServerName)
            throw new AppSignInException("That address isn't an OpenWallet server.");

        if (info.ApiVersion > MobileApp.ApiVersion || IsOlder(appVersion, info.MinimumAppVersion))
            throw new AppSignInException("This server needs a newer version of the app. Update the app, then try again.");

        if (info.ApiVersion < MobileApp.ApiVersion)
            throw new AppSignInException("This server runs an older OpenWallet. Update the server, then try again.");

        if (info.SetupRequired)
            throw new AppSignInException("This server isn't set up yet. Open it in a browser to create your account.");

        if (address != ServerAddress)
        {
            await ClearSessionAsync();
            ServerAddress = address;
            await storage.SetServerAddressAsync(address);
        }

        return info;
    }

    /// <summary>Signs in with a password (and authenticator code); the result says what's missing when it fails.</summary>
    public async Task<LoginResultDto> SignInAsync(AppLoginDto dto)
    {
        AppLoginResult result = await WithPlainApi(api => api.AppLoginAsync(dto));
        if (result.Tokens != null)
            await CompleteSignInAsync(result.Tokens);

        return result.Result;
    }

    /// <summary>Finishes the browser sign-in: exchanges its one-time code, with the verifier only this app knows.</summary>
    public async Task SignInWithCodeAsync(string code, string codeVerifier)
    {
        AppTokensDto tokens = await WithPlainApi(api => api.RedeemMobileCodeAsync(new RedeemMobileCodeDto { Code = code, CodeVerifier = codeVerifier }))
            ?? throw new AppSignInException("The sign-in expired or wasn't valid. Try again.");
        await CompleteSignInAsync(tokens);
    }

    /// <summary>
    /// A current access token, refreshed first when it's about to expire (or <paramref name="force"/>).
    /// Null when signed out. When the server can't be reached, the current token is returned as it is.
    /// </summary>
    public async Task<string?> GetAccessTokenAsync(bool force = false, CancellationToken cancellationToken = default)
    {
        AppSession? session = Session;
        if (session == null) return null;

        if (!force && session.AccessTokenExpiresAt - RefreshMargin > timeProvider.GetUtcNow())
            return session.AccessToken;

        await refreshLock.WaitAsync(cancellationToken);
        try
        {
            if (Session == null) return null;

            if (Session != session && Session.AccessTokenExpiresAt - RefreshMargin > timeProvider.GetUtcNow())
                return Session.AccessToken;

            try
            {
                AppSession current = Session;
                AppTokensDto? tokens = await WithPlainApi(api => api.RefreshAppTokensAsync(current.RefreshToken, cancellationToken));
                if (tokens == null)
                {
                    await ClearSessionAsync();
                    return null;
                }

                Session = ToSession(tokens, current.Username);
                await storage.SaveSessionAsync(Session);
                return Session.AccessToken;
            }
            catch (HttpRequestException)
            {
                return Session?.AccessToken;
            }
        }
        finally
        {
            refreshLock.Release();
        }
    }

    public Task SignOutAsync() => ClearSessionAsync();

    async Task CompleteSignInAsync(AppTokensDto tokens)
    {
        using HttpClient http = createClient(ServerAddress!);
        http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokens.AccessToken);
        AuthMeDto me = await new ApiClient(http).GetMeAsync()
            ?? throw new AppSignInException("The server didn't accept the sign-in. Try again.");

        Session = ToSession(tokens, me.Username);
        await storage.SaveSessionAsync(Session);
        Changed?.Invoke();
    }

    async Task ClearSessionAsync()
    {
        bool wasSignedIn = Session != null;
        Session = null;
        await storage.ClearSessionAsync();
        if (wasSignedIn)
            Changed?.Invoke();
    }

    AppSession ToSession(AppTokensDto tokens, string username) =>
        new(tokens.AccessToken, tokens.RefreshToken, timeProvider.GetUtcNow().AddSeconds(tokens.ExpiresIn), username);

    async Task<T> WithPlainApi<T>(Func<ApiClient, Task<T>> call)
    {
        if (ServerAddress == null)
            throw new AppSignInException("Choose your server first.");

        using HttpClient http = createClient(ServerAddress);
        return await call(new ApiClient(http));
    }

    /// <summary>Whether version <paramref name="current"/> is older than <paramref name="minimum"/> (e.g. "1.2.0").</summary>
    internal static bool IsOlder(string current, string? minimum) =>
        Version.TryParse(minimum, out Version? min) && Version.TryParse(current, out Version? version) && version < min;
}
