using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NpgsqlTypes;
using OpenWallet.Shared.Models;

namespace OpenWallet.Database.Models;

public class Record : IEntityTypeConfiguration<Record>
{
    public int Id { get; set; }
    public int AccountId { get; set; }
    public Account Account { get; set; } = default!;
    public int? CategoryId { get; set; }
    public Category? Category { get; set; }
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
    
    public void Configure(EntityTypeBuilder<Record> builder)
    {
        builder.Property(r => r.Amount).HasPrecision(18, 4);

        builder.HasOne(r => r.LinkedTransferRecord)
            .WithMany()
            .HasForeignKey(r => r.LinkedTransferRecordId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
