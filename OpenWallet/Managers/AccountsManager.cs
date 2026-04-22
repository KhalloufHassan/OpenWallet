using Microsoft.EntityFrameworkCore;
using OpenWallet.Database;
using OpenWallet.Database.Models;
using OpenWallet.Shared.DTOs;

namespace OpenWallet.Managers;

public class AccountsManager(AppDbContext db)
{
    public async Task<List<AccountDto>> GetAllAsync()
    {
        List<Account> accounts = await db.Accounts.ToListAsync();
        List<AccountDto> result = [];

        foreach (Account account in accounts)
        {
            decimal balance = await ComputeBalanceAsync(account.Id);
            result.Add(MapToDto(account, balance));
        }

        return result;
    }

    public async Task<AccountDto> GetByIdAsync(int id)
    {
        Account account = await db.Accounts.FirstOrDefaultAsync(a => a.Id == id)
            ?? throw new KeyNotFoundException($"Account {id} not found.");

        decimal balance = await ComputeBalanceAsync(id);
        return MapToDto(account, balance);
    }

    public async Task<AccountDto> CreateAsync(CreateAccountDto dto)
    {
        Account account = new()
        {
            Name = dto.Name,
            Currency = dto.Currency,
            InitialAmount = dto.InitialAmount,
            Color = dto.Color
        };

        db.Accounts.Add(account);
        await db.SaveChangesAsync();
        return MapToDto(account, account.InitialAmount);
    }

    public async Task<AccountDto> UpdateAsync(int id, UpdateAccountDto dto)
    {
        Account account = await db.Accounts.FindAsync(id)
            ?? throw new KeyNotFoundException($"Account {id} not found.");

        account.Name = dto.Name;
        account.Currency = dto.Currency;
        account.InitialAmount = dto.InitialAmount;
        account.Color = dto.Color;

        await db.SaveChangesAsync();
        decimal balance = await ComputeBalanceAsync(id);
        return MapToDto(account, balance);
    }

    public async Task DeleteAsync(int id)
    {
        Account account = await db.Accounts.FindAsync(id)
            ?? throw new KeyNotFoundException($"Account {id} not found.");

        db.Accounts.Remove(account);
        await db.SaveChangesAsync();
    }

    public async Task<decimal> ComputeBalanceAsync(int accountId)
    {
        Account account = await db.Accounts.FindAsync(accountId)
            ?? throw new KeyNotFoundException($"Account {accountId} not found.");

        decimal income = await db.Records
            .Where(r => r.AccountId == accountId && r.Type == Shared.Models.RecordType.Income)
            .SumAsync(r => r.Amount);

        decimal transferIn = await db.Records
            .Where(r => r.AccountId == accountId && r.Type == Shared.Models.RecordType.Transfer && r.Amount > 0)
            .SumAsync(r => r.Amount);

        decimal expense = await db.Records
            .Where(r => r.AccountId == accountId && r.Type == Shared.Models.RecordType.Expense)
            .SumAsync(r => r.Amount);

        decimal transferOut = await db.Records
            .Where(r => r.AccountId == accountId && r.Type == Shared.Models.RecordType.Transfer && r.Amount < 0)
            .SumAsync(r => r.Amount);

        return account.InitialAmount + income + transferIn + expense + transferOut;
    }

    private static AccountDto MapToDto(Account account, decimal balance) => new()
    {
        Id = account.Id,
        Name = account.Name,
        Currency = account.Currency,
        InitialAmount = account.InitialAmount,
        Color = account.Color,
        CurrentBalance = balance
    };
}
