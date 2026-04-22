namespace OpenWallet.Database.Models;

public class RecordTag
{
    public int RecordId { get; set; }
    public Record Record { get; set; } = default!;
    public int TagId { get; set; }
    public Tag Tag { get; set; } = default!;
}
