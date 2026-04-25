using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace OpenWallet.Database.Models;

public class Account : IEntityTypeConfiguration<Account>
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Currency { get; set; } = string.Empty;
    public decimal InitialAmount { get; set; }
    public string Color { get; set; } = "#6c757d";
    public List<Record> Records { get; set; } = [];

    public void Configure(EntityTypeBuilder<Account> builder)
    {
        builder.Property(a => a.InitialAmount).HasPrecision(18, 4);
    }
}
