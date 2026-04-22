using NpgsqlTypes;
using OpenWallet.Shared.Models;

namespace OpenWallet.Database.Models;

public class Record
{
    public int Id { get; set; }
    public int AccountId { get; set; }
    public Account Account { get; set; } = default!;
    public int CategoryId { get; set; }
    public Category Category { get; set; } = default!;
    public RecordType Type { get; set; }
    public decimal Amount { get; set; }
    public DateTime DateTime { get; set; }
    public string Notes { get; set; } = string.Empty;
    public NpgsqlPoint? Location { get; set; }
    public int? StoreId { get; set; }
    public Store? Store { get; set; }
    public int? LinkedTransferRecordId { get; set; }
    public Record LinkedTransferRecord { get; set; } = default!;
    public List<RecordTag> RecordTags { get; set; } = [];
    public List<Attachment> Attachments { get; set; } = [];
}
