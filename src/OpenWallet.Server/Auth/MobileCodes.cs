using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;

namespace OpenWallet.Auth;

/// <summary>
/// One-time codes for the mobile app's browser sign-in (PKCE). A code is the user id, security stamp
/// and code challenge, encrypted by data protection and valid for two minutes, so no table is needed.
/// It only turns into tokens together with the verifier the app kept to itself.
/// </summary>
public sealed class MobileCodes(IDataProtectionProvider dataProtection, TimeProvider timeProvider)
{
    public static readonly TimeSpan Lifetime = TimeSpan.FromMinutes(2);

    readonly ITimeLimitedDataProtector protector =
        dataProtection.CreateProtector("OpenWallet.MobileCode").ToTimeLimitedDataProtector();

    public static string ChallengeFor(string verifier) =>
        WebEncoders.Base64UrlEncode(SHA256.HashData(Encoding.ASCII.GetBytes(verifier)));

    public async Task<string> IssueAsync(IdentityUser user, string challenge, UserManager<IdentityUser> userManager)
    {
        CodePayload payload = new(user.Id, await userManager.GetSecurityStampAsync(user), challenge);
        return protector.Protect(JsonSerializer.Serialize(payload), timeProvider.GetUtcNow() + Lifetime);
    }

    /// <summary>The user the code was issued to, or null when the code or verifier is wrong, expired or out of date.</summary>
    public async Task<IdentityUser?> RedeemAsync(string code, string verifier, UserManager<IdentityUser> userManager)
    {
        CodePayload? payload;
        try
        {
            payload = JsonSerializer.Deserialize<CodePayload>(protector.Unprotect(code, out DateTimeOffset expiration));
            if (payload is null || expiration < timeProvider.GetUtcNow())
                return null;
        }
        catch (Exception ex) when (ex is CryptographicException or JsonException or FormatException)
        {
            return null;
        }

        byte[] expected = Encoding.ASCII.GetBytes(payload.Challenge ?? string.Empty);
        byte[] actual = Encoding.ASCII.GetBytes(ChallengeFor(verifier));
        if (!CryptographicOperations.FixedTimeEquals(expected, actual))
            return null;

        IdentityUser? user = await userManager.FindByIdAsync(payload.UserId);
        return user != null && await userManager.GetSecurityStampAsync(user) == payload.SecurityStamp ? user : null;
    }

    sealed record CodePayload(string UserId, string SecurityStamp, string Challenge);
}
