using System.Reflection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using OpenWallet.Auth;
using OpenWallet.Shared.Constants;
using OpenWallet.Shared.DTOs;

namespace OpenWallet.Controllers;

[ApiController]
[Route("api/server-info")]
public class ServerInfoController(UserManager<IdentityUser> userManager, IOptions<AppClientOptions> appOptions) : ControllerBase
{
    /// <summary>What the mobile app checks before signing in: that this is an OpenWallet server it can work with.</summary>
    [HttpGet]
    public async Task<ActionResult<ServerInfoDto>> Get() => Ok(new ServerInfoDto
    {
        Name = MobileApp.ServerName,
        Version = Assembly.GetExecutingAssembly().GetName().Version?.ToString(3) ?? string.Empty,
        ApiVersion = MobileApp.ApiVersion,
        MinimumAppVersion = string.IsNullOrWhiteSpace(appOptions.Value.MinimumVersion) ? null : appOptions.Value.MinimumVersion,
        SetupRequired = !await userManager.Users.AnyAsync()
    });
}
