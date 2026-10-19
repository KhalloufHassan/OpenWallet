using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace OpenWallet.Database.Models;

public class TemplateTag : IEntityTypeConfiguration<TemplateTag>
{
    public int TemplateId { get; set; }
    public Template Template { get; set; } = default!;
    public int TagId { get; set; }
    public Tag Tag { get; set; } = default!;

    public void Configure(EntityTypeBuilder<TemplateTag> builder)
    {
        builder.HasKey(tt => new { tt.TemplateId, tt.TagId });
    }
}
