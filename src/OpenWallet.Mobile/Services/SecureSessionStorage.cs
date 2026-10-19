using System.Text.Json;
using OpenWallet.Mobile.Core.Auth;

namespace OpenWallet.Mobile.Services;

/// <summary>The server address in the app's preferences; the session (tokens) in Android's secure storage.</summary>
public sealed class SecureSessionStorage : ISessionStorage
{
    const string ServerKey = "openwallet.server";
    const string SessionKey = "openwallet.session";

    public Task<string?> GetServerAddressAsync() => Task.FromResult(Preferences.Default.Get<string?>(ServerKey, null));

    public Task SetServerAddressAsync(string address)
    {
        Preferences.Default.Set(ServerKey, address);
        return Task.CompletedTask;
    }

    public async Task<AppSession?> LoadSessionAsync()
    {
        try
        {
            string? json = await SecureStorage.Default.GetAsync(SessionKey);
            return json == null ? null : JsonSerializer.Deserialize<AppSession>(json);
        }
        catch (Exception ex) when (ex is JsonException or System.Security.Cryptography.CryptographicException
#if ANDROID
            or Java.Lang.Exception
#endif
        )
        {
            SecureStorage.Default.Remove(SessionKey);
            return null;
        }
    }

    public Task SaveSessionAsync(AppSession session) =>
        SecureStorage.Default.SetAsync(SessionKey, JsonSerializer.Serialize(session));

    public Task ClearSessionAsync()
    {
        SecureStorage.Default.Remove(SessionKey);
        return Task.CompletedTask;
    }
}
