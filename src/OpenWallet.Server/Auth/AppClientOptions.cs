namespace OpenWallet.Auth;

/// <summary>Settings for the mobile app's sign-in (configuration section "Apps").</summary>
public sealed class AppClientOptions
{
    public const string Section = "Apps";

    /// <summary>How long an app's access token works. A password change takes effect within this time.</summary>
    public int AccessTokenMinutes { get; set; } = 30;

    /// <summary>How long an app stays signed in without being used.</summary>
    public int RefreshTokenDays { get; set; } = 90;

    /// <summary>The oldest mobile app version this server accepts, e.g. "1.2.0"; empty for any.</summary>
    public string? MinimumVersion { get; set; }
}
