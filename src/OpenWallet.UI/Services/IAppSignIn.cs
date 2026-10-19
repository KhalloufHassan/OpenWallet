using OpenWallet.Shared.DTOs;

namespace OpenWallet.UI.Services;

/// <summary>
/// Signing in from the mobile app's own sign-in page (<c>/app-login</c>). The web app signs in on its
/// login page instead and doesn't provide this.
/// </summary>
public interface IAppSignIn
{
    /// <summary>The server the app talks to, e.g. "https://wallet.example.org/"; null until chosen.</summary>
    string? ServerAddress { get; }

    /// <summary>
    /// Checks that an OpenWallet server answers at the address (HTTPS only) and that this app works with it,
    /// then remembers it. Throws <see cref="AppSignInException"/> with a message for the user otherwise.
    /// </summary>
    Task<ServerInfoDto> SetServerAsync(string address);

    /// <summary>Signs in with a username, password and, when the account needs it, an authenticator code.</summary>
    Task<LoginResultDto> SignInAsync(AppLoginDto dto);

    /// <summary>Signs in on the server's login page in the system browser, where passkeys work.</summary>
    Task SignInWithBrowserAsync();
}

public sealed class AppSignInException(string message) : Exception(message);
