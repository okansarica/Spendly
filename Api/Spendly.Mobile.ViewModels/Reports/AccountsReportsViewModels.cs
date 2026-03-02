// CHANGED_BY_AI: 2026-03-02 - Add account report view models
namespace Spendly.Mobile.ViewModels.Reports;

public class AccountsOverviewResponseViewModel
{
    public ReportSummaryViewModel Summary { get; set; } = new();
    public List<AccountDistributionViewModel> AccountDistribution { get; set; } = new();
    public List<AccountListItemViewModel> Accounts { get; set; } = new();
}

public class AccountDistributionViewModel
{
    public string AccountId { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public decimal CurrentMonthToDateTotal { get; set; }
    public decimal PercentageOfTotal { get; set; }
}

public class AccountListItemViewModel
{
    public string AccountId { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public decimal CurrentMonthToDateTotal { get; set; }
    public decimal PreviousMonthSamePeriodTotal { get; set; }
    public decimal DifferenceAmount { get; set; }
    public decimal PercentageChange { get; set; }
}

public class AccountDetailResponseViewModel
{
    public AccountSummaryViewModel AccountSummary { get; set; } = new();
    public List<AccountCategoryTotalViewModel> Categories { get; set; } = new();
}

public class AccountSummaryViewModel
{
    public string AccountId { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal TotalAmount { get; set; }
    public AccountComparisonViewModel Comparison { get; set; } = new();
}

public class AccountComparisonViewModel
{
    public decimal PreviousMonthSamePeriodTotal { get; set; }
    public decimal DifferenceAmount { get; set; }
    public decimal PercentageChange { get; set; }
    public string Trend { get; set; } = string.Empty;
    public bool IsNewSpending { get; set; }
}

public class AccountCategoryTotalViewModel
{
    public string CategoryId { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
}

