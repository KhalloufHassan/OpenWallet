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
        DateTime end = to.HasValue ? DateTime.SpecifyKind(to.Value, DateTimeKind.Utc)
            : start.AddMonths(1).AddTicks(-1);

        List<AccountDto> accounts = await accountsManager.GetAllAsync();
        decimal totalBalance = accounts.Sum(a => a.CurrentBalance);

        List<RecordDto> recentRecords = await recordsManager.GetRecentAsync(10);

        List<CategoryExpenseDto> expensesByCategory = await GetExpensesByCategoryAsync(start, end);
        List<TagExpenseDto> expensesByTag = await GetExpensesByTagAsync(start, end);
        List<BalanceTrendDto> balanceTrend = await GetBalanceTrendAsync(start, end);

        return new DashboardDto
        {
            Accounts = accounts,
            TotalBalance = totalBalance,
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
            .Where(r => r.Type == RecordType.Expense && r.DateTime >= from && r.DateTime <= to)
            .ToListAsync();

        decimal total = expenses.Sum(r => Math.Abs(r.Amount));
        if (total == 0) return [];

        return expenses
            .Where(r => r.Category != null)
            .GroupBy(r => r.Category!)
            .Select(g => new CategoryExpenseDto
            {
                CategoryId = g.Key.Id,
                CategoryName = g.Key.Name,
                CategoryColor = g.Key.Color,
                CategoryIcon = g.Key.Icon,
                Amount = Math.Abs(g.Sum(r => r.Amount)),
                Percentage = Math.Round(Math.Abs(g.Sum(r => r.Amount)) / total * 100, 2)
            })
            .OrderByDescending(c => c.Amount)
            .ToList();
    }

    public async Task<List<TagExpenseDto>> GetExpensesByTagAsync(DateTime from, DateTime to)
    {
        List<Record> expenses = await db.Records
            .Include(r => r.RecordTags).ThenInclude(rt => rt.Tag)
            .Where(r => r.Type == RecordType.Expense && r.DateTime >= from && r.DateTime <= to)
            .ToListAsync();

        decimal total = expenses.Sum(r => Math.Abs(r.Amount));
        if (total == 0) return [];

        return expenses
            .SelectMany(r => r.RecordTags.Select(rt => new { rt.Tag, r.Amount }))
            .GroupBy(x => x.Tag)
            .Select(g => new TagExpenseDto
            {
                TagId = g.Key.Id,
                TagName = g.Key.Name,
                Amount = Math.Abs(g.Sum(x => x.Amount)),
                Percentage = Math.Round(Math.Abs(g.Sum(x => x.Amount)) / total * 100, 2)
            })
            .OrderByDescending(t => t.Amount)
            .ToList();
    }

    public async Task<List<BalanceTrendDto>> GetBalanceTrendAsync(DateTime from, DateTime to)
    {
        DateTime start = DateTime.SpecifyKind(from.Date, DateTimeKind.Utc);
        DateTime end   = DateTime.SpecifyKind(to.Date,   DateTimeKind.Utc);

        List<Account> accounts = await db.Accounts.ToListAsync();
        decimal baseBalance = accounts.Sum(a => a.InitialAmount);

        List<Record> records = await db.Records
            .Where(r => r.DateTime >= start && r.DateTime < end.AddDays(1))
            .OrderBy(r => r.DateTime)
            .ToListAsync();

        decimal runningBalance = baseBalance + await db.Records
            .Where(r => r.DateTime < start)
            .SumAsync(r => r.Amount);

        List<BalanceTrendDto> trend = [];
        int days = (int)(end - start).TotalDays;

        for (int i = 0; i <= days; i++)
        {
            DateTime date = start.AddDays(i);
            decimal dayDelta = records
                .Where(r => r.DateTime.Date == date)
                .Sum(r => r.Amount);
            runningBalance += dayDelta;
            trend.Add(new BalanceTrendDto { Date = date, Balance = runningBalance });
        }

        return trend;
    }
}
