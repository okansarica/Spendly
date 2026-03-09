// CHANGED_BY_AI: 2026-03-02 - Expand homepage response model
namespace Spendly.Mobile.ViewModels.Homepage;

public class HomepageResponseViewModel
{
    public decimal CurrentMonthTotalSpending { get; set; }
    public decimal PreviousMonthTotalSpending { get; set; }
    public MidMonthComparisonViewModel MidMonthComparison { get; set; }
    public List<SpendingByAccountViewModel> SpendingByAccountCurrentMonth { get; set; }
    public List<SpendingByAccountViewModel> SpendingByAccountPreviousMonth { get; set; }
    public List<SpendingByCategoryViewModel> SpendingByCategoryCurrentMonth { get; set; }
    public List<SpendingByCategoryViewModel> SpendingByCategoryPreviousMonth { get; set; }
    public List<MonthlySpendingTrendViewModel> SixMonthTrend { get; set; }
    public List<LatestExpenseViewModel> LatestExpenses { get; set; }
    public WeeklySnapshotViewModel WeeklySnapshot { get; set; }
    public TopSpendingCategoryViewModel TopSpendingCategory { get; set; }
    public HighestSingleExpenseViewModel HighestSingleExpense { get; set; }
    public MostUsedAccountViewModel MostUsedAccount { get; set; }
    public DailyAverageViewModel DailyAverage { get; set; }
}

public class MidMonthComparisonViewModel
{
    public bool IsIncreased { get; set; }
    public decimal PercentageChange { get; set; }
}

public class SpendingByAccountViewModel
{
    public string AccountId { get; set; }
    public string AccountName { get; set; }
    public decimal Amount { get; set; }
    public decimal PercentageOfTotal { get; set; }
}

public class SpendingByCategoryViewModel
{
    public string CategoryId { get; set; }
    public string CategoryName { get; set; }
    public decimal Amount { get; set; }
    public decimal PercentageOfTotal { get; set; }
}

public class MonthlySpendingTrendViewModel
{
    public int Year { get; set; }
    public int Month { get; set; }
    public decimal Amount { get; set; }
    public decimal PreviousMonthAmount { get; set; }
    public decimal PercentageChange { get; set; }
}

public class LatestExpenseViewModel
{
    public string TransactionId { get; set; }
    public DateTime Date { get; set; }
    public decimal Amount { get; set; }
    public string CategoryName { get; set; }
    public string? MerchantName { get; set; }
    public string AccountName { get; set; }
    public string TransactionName { get; set; } = string.Empty;
}

public class WeeklySnapshotViewModel
{
    public decimal ThisWeekTotal { get; set; }
    public decimal PreviousWeekTotal { get; set; }
    public decimal PercentageChange { get; set; }
    public bool IsIncreased { get; set; }
}

public class TopSpendingCategoryViewModel
{
    public string CategoryName { get; set; }
    public decimal Amount { get; set; }
    public decimal PercentageOfTotal { get; set; }
}

public class HighestSingleExpenseViewModel
{
    public string? MerchantName { get; set; }
    public decimal Amount { get; set; }
    public DateTime Date { get; set; }
}

public class MostUsedAccountViewModel
{
    public string AccountName { get; set; }
    public decimal PercentageShare { get; set; }
}

public class DailyAverageViewModel
{
    public decimal CurrentMonthAverage { get; set; }
    public decimal PreviousMonthAverage { get; set; }
    public decimal PercentageChange { get; set; }
    public bool IsIncreased { get; set; }
}
