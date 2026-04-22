using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OpenWallet.Database;
using OpenWallet.Database.Models;
using OpenWallet.Shared.DTOs;

namespace OpenWallet.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(
    UserManager<IdentityUser> userManager,
    SignInManager<IdentityUser> signInManager,
    AppDbContext db) : ControllerBase
{
    /// <summary>Returns the currently authenticated user's username.</summary>
    [HttpGet("me")]
    [Authorize]
    public IActionResult GetMe() =>
        Ok(new AuthMeDto { Username = User.Identity!.Name! });

    /// <summary>Authenticates with username and password. Returns whether 2FA is required.</summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto dto)
    {
        IdentityUser? user = await userManager.FindByNameAsync(dto.Username);
        if (user == null)
            return Ok(new LoginResultDto { Error = "Invalid credentials" });

        Microsoft.AspNetCore.Identity.SignInResult result =
            await signInManager.PasswordSignInAsync(user, dto.Password, isPersistent: true, lockoutOnFailure: false);

        if (result.RequiresTwoFactor)
            return Ok(new LoginResultDto { RequiresTwoFactor = true, Username = user.UserName! });

        if (!result.Succeeded)
            return Ok(new LoginResultDto { Error = "Invalid credentials" });

        return Ok(new LoginResultDto { Succeeded = true, Username = user.UserName! });
    }

    /// <summary>Verifies a TOTP code to complete a two-factor login.</summary>
    [HttpPost("totp/verify")]
    public async Task<IActionResult> VerifyTotp(VerifyTotpDto dto)
    {
        Microsoft.AspNetCore.Identity.SignInResult result =
            await signInManager.TwoFactorAuthenticatorSignInAsync(dto.Code, isPersistent: true, rememberClient: false);

        if (!result.Succeeded)
            return Ok(new LoginResultDto { Error = "Invalid code" });

        IdentityUser? user = await signInManager.GetTwoFactorAuthenticationUserAsync();
        return Ok(new LoginResultDto { Succeeded = true, Username = user?.UserName ?? string.Empty });
    }

    /// <summary>Signs out the current user.</summary>
    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        await signInManager.SignOutAsync();
        return Ok();
    }

    /// <summary>Changes the password for the authenticated user.</summary>
    [HttpPost("change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword(ChangePasswordDto dto)
    {
        IdentityUser? user = await userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        IdentityResult result = await userManager.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword);
        if (!result.Succeeded)
            return BadRequest(new { error = result.Errors.FirstOrDefault()?.Description });

        return Ok();
    }

    /// <summary>Returns the current security configuration (TOTP enabled, passkeys).</summary>
    [HttpGet("security")]
    [Authorize]
    public async Task<IActionResult> GetSecurity()
    {
        IdentityUser? user = await userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        bool totpEnabled = await userManager.GetTwoFactorEnabledAsync(user);
        List<PasskeyCredential> passkeys = db.PasskeyCredentials
            .Where(p => p.UserId == user.Id)
            .ToList();

        return Ok(new SecurityStatusDto
        {
            TotpEnabled = totpEnabled,
            Passkeys = passkeys.Select(p => new PasskeyInfoDto
            {
                Id = p.Id,
                Name = p.Name,
                CreatedAt = p.CreatedAt
            }).ToList()
        });
    }

    /// <summary>Generates a new TOTP authenticator key and returns the setup info.</summary>
    [HttpPost("totp/setup")]
    [Authorize]
    public async Task<IActionResult> SetupTotp()
    {
        IdentityUser? user = await userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        await userManager.ResetAuthenticatorKeyAsync(user);
        string? key = await userManager.GetAuthenticatorKeyAsync(user);
        if (key == null) return StatusCode(500);

        string formattedKey = FormatKey(key);
        string qrCodeUri = GenerateQrCodeUri(user.UserName!, key);
        return Ok(new TotpSetupDto { Key = formattedKey, QrCodeUri = qrCodeUri });
    }

    /// <summary>Enables TOTP after verifying the provided code.</summary>
    [HttpPost("totp/enable")]
    [Authorize]
    public async Task<IActionResult> EnableTotp(VerifyTotpDto dto)
    {
        IdentityUser? user = await userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        bool valid = await userManager.VerifyTwoFactorTokenAsync(
            user, userManager.Options.Tokens.AuthenticatorTokenProvider, dto.Code);
        if (!valid)
            return BadRequest(new { error = "Invalid verification code" });

        await userManager.SetTwoFactorEnabledAsync(user, true);
        return Ok();
    }

    /// <summary>Disables TOTP for the authenticated user.</summary>
    [HttpPost("totp/disable")]
    [Authorize]
    public async Task<IActionResult> DisableTotp()
    {
        IdentityUser? user = await userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        await userManager.SetTwoFactorEnabledAsync(user, false);
        await userManager.ResetAuthenticatorKeyAsync(user);
        return Ok();
    }

    static string FormatKey(string key)
    {
        System.Text.StringBuilder result = new();
        int currentPosition = 0;
        while (currentPosition + 4 < key.Length)
        {
            result.Append(key.AsSpan(currentPosition, 4)).Append(' ');
            currentPosition += 4;
        }
        if (currentPosition < key.Length)
            result.Append(key.AsSpan(currentPosition));
        return result.ToString().ToLowerInvariant();
    }

    static string GenerateQrCodeUri(string email, string key) =>
        $"otpauth://totp/OpenWallet:{Uri.EscapeDataString(email)}?secret={key}&issuer=OpenWallet&digits=6";
}
