namespace OpenWallet.Shared.DTOs;

public class AccountDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Currency { get; set; } = string.Empty;
    public decimal InitialAmount { get; set; }
    public string Color { get; set; } = "#6c757d";
    public decimal CurrentBalance { get; set; }
}

public class CreateAccountDto
{
    public string Name { get; set; } = string.Empty;
    public string Currency { get; set; } = string.Empty;
    public decimal InitialAmount { get; set; }
    public string Color { get; set; } = "#6c757d";
}

public class UpdateAccountDto
{
    public string Name { get; set; } = string.Empty;
    public string Currency { get; set; } = string.Empty;
    public decimal InitialAmount { get; set; }
    public string Color { get; set; } = "#6c757d";
}
