namespace OpenWallet.Database.Models;

public class Debt
{
    public int Id { get; set; }
    public string PartyName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public bool IsLending { get; set; }
    public string Notes { get; set; } = string.Empty;
    public bool IsResolved { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<DebtRecord> DebtRecords { get; set; } = [];
}
