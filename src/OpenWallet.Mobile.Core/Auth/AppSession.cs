namespace OpenWallet.Mobile.Core.Auth;

/// <summary>A signed-in session of the app: its tokens, and who is signed in.</summary>
public sealed record AppSession(string AccessToken, string RefreshToken, DateTimeOffset AccessTokenExpiresAt, string Username);
