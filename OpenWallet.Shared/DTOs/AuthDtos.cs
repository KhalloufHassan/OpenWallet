namespace OpenWallet.Shared.DTOs;

public class LoginDto
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class SetupDto
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public string AccountCurrency { get; set; } = "USD";
    public decimal InitialBalance { get; set; }
    public string AccountColor { get; set; } = "#388bfd";
}

public class SetupStatusDto
{
    public bool IsSetupDone { get; set; }
}

public class AuthMeDto
{
    public string Username { get; set; } = string.Empty;
}

public class ChangePasswordDto
{
    public string CurrentPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}

public class LoginResultDto
{
    public bool Succeeded { get; set; }
    public bool RequiresTwoFactor { get; set; }
    public string Username { get; set; } = string.Empty;
    public string? Error { get; set; }
}

public class VerifyTotpDto
{
    public string Code { get; set; } = string.Empty;
}

public class TotpSetupDto
{
    public string Key { get; set; } = string.Empty;
    public string QrCodeUri { get; set; } = string.Empty;
}

public class SecurityStatusDto
{
    public bool TotpEnabled { get; set; }
    public List<PasskeyInfoDto> Passkeys { get; set; } = [];
}

public class PasskeyInfoDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class RegisterPasskeyNameDto
{
    public string Name { get; set; } = string.Empty;
}
