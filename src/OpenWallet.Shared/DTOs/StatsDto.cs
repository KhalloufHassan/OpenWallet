namespace OpenWallet.Shared.DTOs;

public class DashboardDto
{
    public List<AccountDto> Accounts { get; set; } = [];
    public List<CurrencyTotalDto> TotalsByCurrency { get; set; } = [];
    public List<RecordDto> RecentRecords { get; set; } = [];
    public List<CategoryExpenseDto> ExpensesByCategory { get; set; } = [];
    public List<TagExpenseDto> ExpensesByTag { get; set; } = [];
    public List<BalanceTrendDto> BalanceTrend { get; set; } = [];
}

public class CurrencyTotalDto
{
    public string Currency { get; set; } = string.Empty;
    public decimal Total { get; set; }
}

public class CategoryExpenseDto
{
    public string Currency { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string CategoryColor { get; set; } = string.Empty;
    public string CategoryIcon { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal Percentage { get; set; }
}

public class TagExpenseDto
{
    public string Currency { get; set; } = string.Empty;
    public int TagId { get; set; }
    public string TagName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal Percentage { get; set; }
}

public class BalanceTrendDto
{
    public DateTime Date { get; set; }
    public string Currency { get; set; } = string.Empty;
    public decimal Balance { get; set; }
}
