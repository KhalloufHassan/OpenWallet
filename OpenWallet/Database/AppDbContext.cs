using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using OpenWallet.Database.Models;

namespace OpenWallet.Database;

public class AppDbContext(DbContextOptions<AppDbContext> options) : IdentityDbContext<IdentityUser>(options)
{
    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Tag> Tags => Set<Tag>();
    public DbSet<Store> Stores => Set<Store>();
    public DbSet<Record> Records => Set<Record>();
    public DbSet<RecordTag> RecordTags => Set<RecordTag>();
    public DbSet<Attachment> Attachments => Set<Attachment>();
    public DbSet<Template> Templates => Set<Template>();
    public DbSet<TemplateTag> TemplateTags => Set<TemplateTag>();
    public DbSet<Debt> Debts => Set<Debt>();
    public DbSet<DebtRecord> DebtRecords => Set<DebtRecord>();
    public DbSet<PasskeyCredential> PasskeyCredentials => Set<PasskeyCredential>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<RecordTag>().HasKey(rt => new { rt.RecordId, rt.TagId });
        modelBuilder.Entity<TemplateTag>().HasKey(tt => new { tt.TemplateId, tt.TagId });
        modelBuilder.Entity<DebtRecord>().HasKey(dr => new { dr.DebtId, dr.RecordId });

        modelBuilder.Entity<Category>()
            .HasOne(c => c.ParentCategory)
            .WithMany(c => c.SubCategories)
            .HasForeignKey(c => c.ParentCategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Record>()
            .HasOne(r => r.LinkedTransferRecord)
            .WithMany()
            .HasForeignKey(r => r.LinkedTransferRecordId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Record>()
            .Property(r => r.Amount)
            .HasPrecision(18, 4);

        modelBuilder.Entity<Account>()
            .Property(a => a.InitialAmount)
            .HasPrecision(18, 4);

        modelBuilder.Entity<Template>()
            .Property(t => t.Amount)
            .HasPrecision(18, 4);

        modelBuilder.Entity<Debt>()
            .Property(d => d.Amount)
            .HasPrecision(18, 4);
    }
}
