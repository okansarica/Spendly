namespace Spendly.Mobile.ViewModels.Homepage;

public class HomepageResponseViewModel
{
    public decimal PreviousMonthTotalSpending { get; set; }
    public List<SpendingByAccountViewModel> SpendingByAccount { get; set; }
    public List<SpendingByCategoryViewModel> SpendingByCategory { get; set; }
    public List<MonthlySpendingTrendViewModel> SixMonthTrend { get; set; }
    public List<LatestExpenseViewModel> LatestExpenses { get; set; }
}

public class SpendingByAccountViewModel
{
    public string AccountId { get; set; }
    public string AccountName { get; set; }
    public decimal Amount { get; set; }
}

public class SpendingByCategoryViewModel
{
    public string CategoryId { get; set; }
    public string CategoryName { get; set; }
    public decimal Amount { get; set; }
}

public class MonthlySpendingTrendViewModel
{
    public int Year { get; set; }
    public int Month { get; set; }
    public decimal Amount { get; set; }
}

public class LatestExpenseViewModel
{
    public string TransactionId { get; set; }
    public DateTime Date { get; set; }
    public decimal Amount { get; set; }
    public string CategoryName { get; set; }
    public string MerchantName { get; set; }
    public string AccountName { get; set; }
}

