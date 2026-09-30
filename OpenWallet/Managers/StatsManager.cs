using Microsoft.EntityFrameworkCore;
using OpenWallet.Database;
using OpenWallet.Database.Models;
using OpenWallet.Shared.DTOs;
using OpenWallet.Shared.Models;

namespace OpenWallet.Managers;

public class StatsManager(AppDbContext db, AccountsManager accountsManager, RecordsManager recordsManager)
{
    public async Task<DashboardDto> GetDashboardAsync(DateTime? from = null, DateTime? to = null)
    {
        DateTime now = DateTime.UtcNow;
        DateTime start = from.HasValue ? DateTime.SpecifyKind(from.Value, DateTimeKind.Utc)
            : new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        DateTime end = to.HasValue ? DateTime.SpecifyKind(to.Value.Date, DateTimeKind.Utc)
            : start.AddMonths(1).AddDays(-1);

        List<AccountDto> accounts = await accountsManager.GetAllAsync();
        List<CurrencyTotalDto> totalsByCurrency = accounts
            .GroupBy(a => a.Currency)
            .Select(g => new CurrencyTotalDto { Currency = g.Key, Total = g.Sum(a => a.CurrentBalance) })
            .OrderByDescending(c => c.Total)
            .ToList();

        List<RecordDto> recentRecords = await recordsManager.GetRecentAsync(10, start, end);

        List<CategoryExpenseDto> expensesByCategory = await GetExpensesByCategoryAsync(start, end);
        List<TagExpenseDto> expensesByTag = await GetExpensesByTagAsync(start, end);
        List<BalanceTrendDto> balanceTrend = await GetBalanceTrendAsync(start, end);

        return new DashboardDto
        {
            Accounts = accounts,
            TotalsByCurrency = totalsByCurrency,
            RecentRecords = recentRecords,
            ExpensesByCategory = expensesByCategory,
            ExpensesByTag = expensesByTag,
            BalanceTrend = balanceTrend
        };
    }

    public async Task<List<CategoryExpenseDto>> GetExpensesByCategoryAsync(DateTime from, DateTime to)
    {
        List<Record> expenses = await db.Records
            .Include(r => r.Category)
            .Include(r => r.Account)
            .Where(r => r.Type == RecordType.Expense && r.DateTime >= from && r.DateTime < to.Date.AddDays(1))
            .ToListAsync();

        Dictionary<string, decimal> totalByCurrency = expenses
            .GroupBy(r => r.Account.Currency)
            .ToDictionary(g => g.Key, g => g.Sum(r => Math.Abs(r.Amount)));

        return expenses
            .Where(r => r.Category != null)
            .GroupBy(r => new { Currency = r.Account.Currency, Category = r.Category! })
            .Select(g => new CategoryExpenseDto
            {
                Currency = g.Key.Currency,
                CategoryId = g.Key.Category.Id,
                CategoryName = g.Key.Category.Name,
                CategoryColor = g.Key.Category.Color,
                CategoryIcon = g.Key.Category.Icon,
                Amount = Math.Abs(g.Sum(r => r.Amount)),
                Percentage = totalByCurrency[g.Key.Currency] == 0 ? 0
                    : Math.Round(Math.Abs(g.Sum(r => r.Amount)) / totalByCurrency[g.Key.Currency] * 100, 2)
            })
            .OrderByDescending(c => c.Amount)
            .ToList();
    }

    public async Task<List<TagExpenseDto>> GetExpensesByTagAsync(DateTime from, DateTime to)
    {
        List<Record> expenses = await db.Records
            .Include(r => r.RecordTags).ThenInclude(rt => rt.Tag)
            .Include(r => r.Account)
            .Where(r => r.Type == RecordType.Expense && r.DateTime >= from && r.DateTime < to.Date.AddDays(1))
            .ToListAsync();

        Dictionary<string, decimal> totalByCurrency = expenses
            .GroupBy(r => r.Account.Currency)
            .ToDictionary(g => g.Key, g => g.Sum(r => Math.Abs(r.Amount)));

        return expenses
            .SelectMany(r => r.RecordTags.Select(rt => new { Currency = r.Account.Currency, rt.Tag, r.Amount }))
            .GroupBy(x => new { x.Currency, x.Tag })
            .Select(g => new TagExpenseDto
            {
                Currency = g.Key.Currency,
                TagId = g.Key.Tag.Id,
                TagName = g.Key.Tag.Name,
                Amount = Math.Abs(g.Sum(x => x.Amount)),
                Percentage = totalByCurrency[g.Key.Currency] == 0 ? 0
                    : Math.Round(Math.Abs(g.Sum(x => x.Amount)) / totalByCurrency[g.Key.Currency] * 100, 2)
            })
            .OrderByDescending(t => t.Amount)
            .ToList();
    }

    public async Task<List<BalanceTrendDto>> GetBalanceTrendAsync(DateTime from, DateTime to)
    {
        DateTime start = DateTime.SpecifyKind(from.Date, DateTimeKind.Utc);
        DateTime end   = DateTime.SpecifyKind(to.Date,   DateTimeKind.Utc);

        List<Account> accounts = await db.Accounts.ToListAsync();
        Dictionary<int, string> currencyByAccount = accounts.ToDictionary(a => a.Id, a => a.Currency);
        List<string> currencies = accounts.Select(a => a.Currency).Distinct().ToList();

        Dictionary<string, decimal> running = accounts
            .GroupBy(a => a.Currency)
            .ToDictionary(g => g.Key, g => g.Sum(a => a.InitialAmount));

        Dictionary<int, decimal> priorByAccount = await db.Records
            .Where(r => r.DateTime < start)
            .GroupBy(r => r.AccountId)
            .Select(g => new { AccountId = g.Key, Sum = g.Sum(r => r.Amount) })
            .ToDictionaryAsync(x => x.AccountId, x => x.Sum);

        foreach (KeyValuePair<int, decimal> kv in priorByAccount)
            if (currencyByAccount.TryGetValue(kv.Key, out string? cur))
                running[cur] += kv.Value;

        List<Record> records = await db.Records
            .Where(r => r.DateTime >= start && r.DateTime < end.AddDays(1))
            .ToListAsync();

        Dictionary<(DateTime Date, string Currency), decimal> deltas = records
            .Where(r => currencyByAccount.ContainsKey(r.AccountId))
            .GroupBy(r => (r.DateTime.Date, Currency: currencyByAccount[r.AccountId]))
            .ToDictionary(g => g.Key, g => g.Sum(r => r.Amount));

        List<BalanceTrendDto> trend = [];
        int days = (int)(end - start).TotalDays;

        for (int i = 0; i <= days; i++)
        {
            DateTime date = start.AddDays(i);
            foreach (string currency in currencies)
            {
                running[currency] += deltas.GetValueOrDefault((date, currency), 0m);
                trend.Add(new BalanceTrendDto { Date = date, Currency = currency, Balance = running[currency] });
            }
        }

        return trend;
    }
}
