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
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
