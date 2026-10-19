using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace OpenWallet.Database.Models;

public class DebtRecord : IEntityTypeConfiguration<DebtRecord>
{
    public int DebtId { get; set; }
    public Debt Debt { get; set; } = default!;
    public int RecordId { get; set; }
    public Record Record { get; set; } = default!;

    public void Configure(EntityTypeBuilder<DebtRecord> builder)
    {
        builder.HasKey(dr => new { dr.DebtId, dr.RecordId });
    }
}
