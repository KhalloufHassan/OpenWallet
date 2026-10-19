using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using OpenWallet.Auth;
using OpenWallet.Shared.Constants;
using OpenWallet.Shared.DTOs;

namespace OpenWallet.Controllers;

/// <summary>
/// Sign-in for the mobile app, with bearer tokens instead of the web app's cookie. Built like
/// Identity's <c>MapIdentityApi</c> login and refresh, without its registration endpoints.
/// </summary>
[ApiController]
[Route("api/auth/app")]
public class AppAuthController(
    UserManager<IdentityUser> userManager,
    SignInManager<IdentityUser> signInManager,
    IOptionsMonitor<BearerTokenOptions> bearerOptions,
    MobileCodes mobileCodes,
    TimeProvider timeProvider) : ControllerBase
{
    /// <summary>
    /// Signs an app in with a username and password, plus the authenticator code when the account has
    /// two-factor authentication. Returns the tokens, or 401 with the reason.
    /// </summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login(AppLoginDto dto)
    {
        signInManager.AuthenticationScheme = IdentityConstants.BearerScheme;
        Microsoft.AspNetCore.Identity.SignInResult result =
            await signInManager.PasswordSignInAsync(dto.Username.Trim(), dto.Password, isPersistent: false, lockoutOnFailure: true);

        if (result.RequiresTwoFactor && !string.IsNullOrWhiteSpace(dto.TwoFactorCode))
        {
            string code = dto.TwoFactorCode.Replace(" ", string.Empty).Replace("-", string.Empty);
            result = await signInManager.TwoFactorAuthenticatorSignInAsync(code, isPersistent: false, rememberClient: false);
            if (result.Succeeded)
                return new EmptyResult();

            return Unauthorized(new LoginResultDto
            {
                RequiresTwoFactor = true,
                Error = result.IsLockedOut ? "Account is locked. Try again later." : "Invalid code. Please try again."
            });
        }

        if (result.Succeeded)
            return new EmptyResult();

        if (result.RequiresTwoFactor)
            return Unauthorized(new LoginResultDto { RequiresTwoFactor = true });

        return Unauthorized(new LoginResultDto
        {
            Error = result.IsLockedOut ? "Account is locked. Try again later." : "Invalid username or password."
        });
    }

    /// <summary>New tokens for a refresh token. Fails after a password change or when the account is locked.</summary>
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh(RefreshTokenDto dto)
    {
        ISecureDataFormat<AuthenticationTicket> protector = bearerOptions.Get(IdentityConstants.BearerScheme).RefreshTokenProtector;
        AuthenticationTicket? ticket = protector.Unprotect(dto.RefreshToken);

        if (ticket?.Properties.ExpiresUtc is not DateTimeOffset expires
            || timeProvider.GetUtcNow() >= expires
            || await signInManager.ValidateSecurityStampAsync(ticket.Principal) is not IdentityUser user
            || !await signInManager.CanSignInAsync(user)
            || await userManager.IsLockedOutAsync(user))
            return Unauthorized();

        ClaimsPrincipal principal = await signInManager.CreateUserPrincipalAsync(user);
        return SignIn(principal, IdentityConstants.BearerScheme);
    }

    /// <summary>
    /// The browser half of the app's passkey sign-in: the web app, signed in with its cookie, asks for a
    /// one-time code after the user confirms on the <c>mobile-login</c> page.
    /// </summary>
    [HttpPost("mobile-code/issue")]
    [Authorize(AuthenticationSchemes = "Identity.Application")]
    public async Task<IActionResult> IssueMobileCode(MobileCodeRequestDto dto)
    {
        if (!MobileApp.IsValidChallenge(dto.Challenge))
            return BadRequest(new { error = "This sign-in link isn't valid. Start again from the app." });

        IdentityUser? user = await userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        return Ok(new MobileCodeDto { Code = await mobileCodes.IssueAsync(user, dto.Challenge, userManager) });
    }

    /// <summary>Exchanges the one-time code from the browser sign-in, with the app's PKCE verifier, for tokens.</summary>
    [HttpPost("mobile-code")]
    public async Task<IActionResult> RedeemMobileCode(RedeemMobileCodeDto dto)
    {
        IdentityUser? user = await mobileCodes.RedeemAsync(dto.Code, dto.CodeVerifier, userManager);
        if (user == null || !await signInManager.CanSignInAsync(user) || await userManager.IsLockedOutAsync(user))
            return Unauthorized();

        ClaimsPrincipal principal = await signInManager.CreateUserPrincipalAsync(user);
        return SignIn(principal, IdentityConstants.BearerScheme);
    }
}
