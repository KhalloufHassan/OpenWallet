using System.Text;
using System.Text.Json;
using Fido2NetLib;
using Fido2NetLib.Objects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using OpenWallet.Database;
using OpenWallet.Database.Models;
using OpenWallet.Shared.DTOs;

namespace OpenWallet.Controllers;

[ApiController]
[Route("api/auth/passkey")]
[Authorize]
public class PasskeysController(
    IFido2 fido2,
    IMemoryCache cache,
    UserManager<IdentityUser> userManager,
    SignInManager<IdentityUser> signInManager,
    AppDbContext db) : ControllerBase
{
    const string LoginCacheKey = "passkey_login_options";

    /// <summary>Begins passkey registration — returns CredentialCreateOptions.</summary>
    [HttpPost("register/options")]
    public async Task<IActionResult> RegisterOptions([FromBody] RegisterPasskeyNameDto dto)
    {
        IdentityUser? user = await userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        List<PublicKeyCredentialDescriptor> existingKeys = db.PasskeyCredentials
            .Where(p => p.UserId == user.Id)
            .AsEnumerable()
            .Select(p => new PublicKeyCredentialDescriptor(p.CredentialId))
            .ToList();

        Fido2User fido2User = new()
        {
            Id = Encoding.UTF8.GetBytes(user.Id),
            Name = user.UserName!,
            DisplayName = user.UserName!
        };

        CredentialCreateOptions options = fido2.RequestNewCredential(new RequestNewCredentialParams
        {
            User = fido2User,
            ExcludeCredentials = existingKeys,
            AuthenticatorSelection = new AuthenticatorSelection
            {
                ResidentKey = ResidentKeyRequirement.Preferred,
                UserVerification = UserVerificationRequirement.Preferred
            },
            AttestationPreference = AttestationConveyancePreference.None
        });

        string cacheKey = $"passkey_reg_{user.Id}";
        cache.Set(cacheKey, (options, dto.Name), TimeSpan.FromMinutes(5));

        return Ok(options);
    }

    /// <summary>Completes passkey registration.</summary>
    [HttpPost("register/complete")]
    public async Task<IActionResult> RegisterComplete([FromBody] AuthenticatorAttestationRawResponse attestation)
    {
        IdentityUser? user = await userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        string cacheKey = $"passkey_reg_{user.Id}";
        if (!cache.TryGetValue(cacheKey, out (CredentialCreateOptions Options, string Name) cached))
            return BadRequest(new { error = "Registration session expired" });
        cache.Remove(cacheKey);

        RegisteredPublicKeyCredential result;
        try
        {
            result = await fido2.MakeNewCredentialAsync(new MakeNewCredentialParams
            {
                AttestationResponse = attestation,
                OriginalOptions = cached.Options,
                IsCredentialIdUniqueToUserCallback = (args, ct) =>
                {
                    byte[] credId = args.CredentialId;
                    bool unique = !db.PasskeyCredentials.Any(p => p.CredentialId == credId);
                    return Task.FromResult(unique);
                }
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }

        PasskeyCredential credential = new()
        {
            UserId = user.Id,
            Name = cached.Name,
            CredentialId = result.Id,
            PublicKey = result.PublicKey,
            SignCount = result.SignCount,
            Transports = JsonSerializer.Serialize(result.Transports ?? []),
            IsBackupEligible = result.IsBackupEligible,
            IsBackedUp = result.IsBackedUp,
            AaGuid = result.AaGuid,
            CreatedAt = DateTime.UtcNow
        };

        db.PasskeyCredentials.Add(credential);
        await db.SaveChangesAsync();

        return Ok(new PasskeyInfoDto { Id = credential.Id, Name = credential.Name, CreatedAt = credential.CreatedAt });
    }

    /// <summary>Deletes a registered passkey.</summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        IdentityUser? user = await userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        PasskeyCredential? credential = db.PasskeyCredentials
            .FirstOrDefault(p => p.Id == id && p.UserId == user.Id);
        if (credential == null) return NotFound();

        db.PasskeyCredentials.Remove(credential);
        await db.SaveChangesAsync();
        return Ok();
    }

    /// <summary>Begins passkey authentication — returns AssertionOptions. Does not require auth.</summary>
    [HttpPost("login/options")]
    [AllowAnonymous]
    public IActionResult LoginOptions()
    {
        List<PublicKeyCredentialDescriptor> allowedCreds = db.PasskeyCredentials
            .AsEnumerable()
            .Select(p => new PublicKeyCredentialDescriptor(p.CredentialId))
            .ToList();

        AssertionOptions options = fido2.GetAssertionOptions(new GetAssertionOptionsParams
        {
            AllowedCredentials = allowedCreds,
            UserVerification = UserVerificationRequirement.Preferred
        });

        cache.Set(LoginCacheKey, options, TimeSpan.FromMinutes(5));
        return Ok(options);
    }

    /// <summary>Completes passkey authentication and signs in. Does not require auth.</summary>
    [HttpPost("login/complete")]
    [AllowAnonymous]
    public async Task<IActionResult> LoginComplete([FromBody] AuthenticatorAssertionRawResponse assertion)
    {
        if (!cache.TryGetValue(LoginCacheKey, out AssertionOptions? options) || options == null)
            return BadRequest(new { error = "Authentication session expired" });
        cache.Remove(LoginCacheKey);

        byte[] credentialId = assertion.RawId;
        List<PasskeyCredential> allCredentials = db.PasskeyCredentials.ToList();
        PasskeyCredential? storedCredential = allCredentials
            .FirstOrDefault(p => p.CredentialId.SequenceEqual(credentialId));
        if (storedCredential == null)
            return BadRequest(new { error = "Credential not found" });

        IdentityUser? user = await userManager.FindByIdAsync(storedCredential.UserId);
        if (user == null) return BadRequest(new { error = "User not found" });

        VerifyAssertionResult assertionResult = await fido2.MakeAssertionAsync(new MakeAssertionParams
        {
            AssertionResponse = assertion,
            OriginalOptions = options,
            StoredPublicKey = storedCredential.PublicKey,
            StoredSignatureCounter = storedCredential.SignCount,
            IsUserHandleOwnerOfCredentialIdCallback = (args, ct) =>
            {
                string userId = Encoding.UTF8.GetString(args.UserHandle);
                bool owns = db.PasskeyCredentials
                    .AsEnumerable()
                    .Any(p => p.CredentialId.SequenceEqual(args.CredentialId) && p.UserId == userId);
                return Task.FromResult(owns);
            }
        });

        storedCredential.SignCount = assertionResult.SignCount;
        await db.SaveChangesAsync();

        await signInManager.SignInAsync(user, isPersistent: true);
        return Ok(new LoginResultDto { Succeeded = true, Username = user.UserName! });
    }
}
