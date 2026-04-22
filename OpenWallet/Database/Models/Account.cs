namespace OpenWallet.Database.Models;

public class Account
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Currency { get; set; } = string.Empty;
    public decimal InitialAmount { get; set; }
    public string Color { get; set; } = "#6c757d";
    public List<Record> Records { get; set; } = [];
}
