namespace OpenWallet.Shared.DTOs;

public class AppLoginDto
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? TwoFactorCode { get; set; }
}

/// <summary>The tokens the server's bearer scheme writes on sign-in (the shape of Identity's AccessTokenResponse).</summary>
public class AppTokensDto
{
    public string TokenType { get; set; } = "Bearer";
    public string AccessToken { get; set; } = string.Empty;
    public long ExpiresIn { get; set; }
    public string RefreshToken { get; set; } = string.Empty;
}

public class RefreshTokenDto
{
    public string RefreshToken { get; set; } = string.Empty;
}

public class MobileCodeRequestDto
{
    public string Challenge { get; set; } = string.Empty;
}

public class MobileCodeDto
{
    public string Code { get; set; } = string.Empty;
}

public class RedeemMobileCodeDto
{
    public string Code { get; set; } = string.Empty;
    public string CodeVerifier { get; set; } = string.Empty;
}

public class ServerInfoDto
{
    public string Name { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public int ApiVersion { get; set; }
    public string? MinimumAppVersion { get; set; }
    public bool SetupRequired { get; set; }
}
