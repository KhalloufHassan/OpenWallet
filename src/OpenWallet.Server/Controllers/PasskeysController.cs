using System.Buffers.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OpenWallet.Shared.DTOs;

namespace OpenWallet.Controllers;

/// <summary>
/// Passkeys through ASP.NET Core Identity. A passkey belongs to the address it was created on, taken
/// from the request's Host header, so behind a reverse proxy the original Host must be passed on.
/// </summary>
[ApiController]
[Route("api/auth/passkey")]
[Authorize]
public class PasskeysController(
    UserManager<IdentityUser> userManager,
    SignInManager<IdentityUser> signInManager,
    ILogger<PasskeysController> logger) : ControllerBase
{
    /// <summary>Begins passkey registration — returns the WebAuthn creation options as JSON.</summary>
    [HttpPost("register/options")]
    public async Task<IActionResult> RegisterOptions()
    {
        IdentityUser? user = await userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        string userName = user.UserName!;
        string optionsJson = await signInManager.MakePasskeyCreationOptionsAsync(new PasskeyUserEntity
        {
            Id = user.Id,
            Name = userName,
            DisplayName = userName
        });
        return Content(optionsJson, "application/json");
    }

    /// <summary>Completes passkey registration and stores the passkey under the given name.</summary>
    [HttpPost("register/complete")]
    public async Task<IActionResult> RegisterComplete(CompletePasskeyRegistrationDto dto)
    {
        IdentityUser? user = await userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        PasskeyAttestationResult attestation;
        try
        {
            attestation = await signInManager.PerformPasskeyAttestationAsync(dto.CredentialJson);
        }
        catch (InvalidOperationException)
        {
            return BadRequest(new { error = "Registration session expired. Please try again." });
        }

        if (!attestation.Succeeded)
            return BadRequest(new { error = $"Could not add the passkey: {attestation.Failure.Message}" });

        attestation.Passkey.Name = dto.Name;
        IdentityResult result = await userManager.AddOrUpdatePasskeyAsync(user, attestation.Passkey);
        if (!result.Succeeded)
            return BadRequest(new { error = "The passkey could not be added to your account." });

        return Ok(ToDto(attestation.Passkey));
    }

    /// <summary>Deletes a registered passkey by its Base64Url credential ID.</summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        IdentityUser? user = await userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        byte[] credentialId;
        try
        {
            credentialId = Base64Url.DecodeFromChars(id);
        }
        catch (FormatException)
        {
            return BadRequest(new { error = "Invalid passkey ID." });
        }

        IdentityResult result = await userManager.RemovePasskeyAsync(user, credentialId);
        return result.Succeeded ? Ok() : NotFound();
    }

    /// <summary>
    /// Begins passkey authentication — returns the WebAuthn request options as JSON. Without a username
    /// the browser offers the passkeys it has for this site.
    /// </summary>
    [HttpPost("login/options")]
    [AllowAnonymous]
    public async Task<IActionResult> LoginOptions([FromQuery] string? username)
    {
        IdentityUser? user = string.IsNullOrWhiteSpace(username) ? null : await userManager.FindByNameAsync(username);
        string optionsJson = await signInManager.MakePasskeyRequestOptionsAsync(user);
        return Content(optionsJson, "application/json");
    }

    /// <summary>Completes passkey authentication and signs in. A passkey also counts as the second factor.</summary>
    [HttpPost("login/complete")]
    [AllowAnonymous]
    public async Task<IActionResult> LoginComplete(PasskeyLoginDto dto)
    {
        PasskeyAssertionResult<IdentityUser> assertion;
        try
        {
            assertion = await signInManager.PerformPasskeyAssertionAsync(dto.CredentialJson);
        }
        catch (InvalidOperationException)
        {
            return Ok(new LoginResultDto { Error = "Passkey sign-in expired. Please try again." });
        }

        if (!assertion.Succeeded)
        {
            logger.LogWarning("Passkey sign-in failed for {Host}: {Reason}", Request.Host, assertion.Failure.Message);
            return Ok(new LoginResultDto
            {
                Error = $"That passkey didn't work: {assertion.Failure.Message} A passkey only works on the address it was created on."
            });
        }

        IdentityUser user = assertion.User;
        if (await userManager.IsLockedOutAsync(user))
            return Ok(new LoginResultDto { Error = "Account is locked. Try again later." });

        await userManager.AddOrUpdatePasskeyAsync(user, assertion.Passkey);
        await signInManager.SignInAsync(user, isPersistent: true, authenticationMethod: "passkey");
        return Ok(new LoginResultDto { Succeeded = true, Username = user.UserName! });
    }

    internal static PasskeyInfoDto ToDto(UserPasskeyInfo passkey) => new()
    {
        Id = Base64Url.EncodeToString(passkey.CredentialId),
        Name = passkey.Name ?? "Passkey",
        CreatedAt = passkey.CreatedAt.UtcDateTime
    };
}
