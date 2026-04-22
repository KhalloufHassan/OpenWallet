namespace OpenWallet.Database.Models;

public class DebtRecord
{
    public int DebtId { get; set; }
    public Debt Debt { get; set; } = default!;
    public int RecordId { get; set; }
    public Record Record { get; set; } = default!;
}
