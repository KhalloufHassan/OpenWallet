using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace OpenWallet.Database.Models;

public class Category : IEntityTypeConfiguration<Category>
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Icon { get; set; } = "bi-tag";
    public string Color { get; set; } = "#6c757d";
    public int? ParentCategoryId { get; set; }
    public Category ParentCategory { get; set; } = default!;
    public List<Category> SubCategories { get; set; } = [];
    public List<Record> Records { get; set; } = [];

    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.HasOne(c => c.ParentCategory)
            .WithMany(c => c.SubCategories)
            .HasForeignKey(c => c.ParentCategoryId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
