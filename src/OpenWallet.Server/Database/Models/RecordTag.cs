using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace OpenWallet.Database.Models;

public class RecordTag : IEntityTypeConfiguration<RecordTag>
{
    public int RecordId { get; set; }
    public Record Record { get; set; } = default!;
    public int TagId { get; set; }
    public Tag Tag { get; set; } = default!;

    public void Configure(EntityTypeBuilder<RecordTag> builder)
    {
        builder.HasKey(rt => new { rt.RecordId, rt.TagId });
    }
}
