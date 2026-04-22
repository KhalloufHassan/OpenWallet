namespace OpenWallet.Shared.DTOs;

public class DashboardDto
{
    public List<AccountDto> Accounts { get; set; } = [];
    public decimal TotalBalance { get; set; }
    public List<RecordDto> RecentRecords { get; set; } = [];
    public List<CategoryExpenseDto> ExpensesByCategory { get; set; } = [];
    public List<TagExpenseDto> ExpensesByTag { get; set; } = [];
    public List<BalanceTrendDto> BalanceTrend { get; set; } = [];
}

public class CategoryExpenseDto
{
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string CategoryColor { get; set; } = string.Empty;
    public string CategoryIcon { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal Percentage { get; set; }
}

public class TagExpenseDto
{
    public int TagId { get; set; }
    public string TagName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal Percentage { get; set; }
}

public class BalanceTrendDto
{
    public DateTime Date { get; set; }
    public decimal Balance { get; set; }
}
