namespace OpenWallet.Shared.Constants;

/// <summary>What the server and the mobile app agree on.</summary>
public static class MobileApp
{
    /// <summary>The name <c>api/server-info</c> answers with, so the app knows it found an OpenWallet server.</summary>
    public const string ServerName = "OpenWallet";

    /// <summary>Raised when the API changes in a way older apps can't handle.</summary>
    public const int ApiVersion = 1;

    /// <summary>The web page the app opens in the browser with <c>?challenge=</c> (the PKCE code challenge) to sign in with a passkey.</summary>
    public const string StartPath = "mobile-login";

    /// <summary>Where the browser sends the one-time code back to the app.</summary>
    public const string CallbackUri = "openwallet://auth";

    /// <summary>A PKCE S256 challenge: the base64url SHA-256 of the verifier, 43 characters.</summary>
    public static bool IsValidChallenge(string? challenge) =>
        challenge is { Length: 43 } && challenge.All(c => char.IsAsciiLetterOrDigit(c) || c is '-' or '_');
}
