// CHANGED_BY_AI: 2026-03-13 - Add merchant reports view models
namespace Spendly.Mobile.ViewModels.Reports;

public class MerchantsReportOverviewResponseViewModel
{
    public ReportSummaryViewModel Summary { get; set; } = new();
    public List<ReportMerchantDistributionViewModel> MerchantDistribution { get; set; } = new();
    public List<ReportMerchantListItemViewModel> Merchants { get; set; } = new();
}

public class ReportMerchantDistributionViewModel
{
    public string MerchantId { get; set; } = string.Empty;
    public string MerchantName { get; set; } = string.Empty;
    public decimal CurrentMonthToDateTotal { get; set; }
    public decimal PercentageOfTotal { get; set; }
}

public class ReportMerchantListItemViewModel
{
    public string MerchantId { get; set; } = string.Empty;
    public string MerchantName { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public decimal CurrentMonthToDateTotal { get; set; }
    public decimal PreviousMonthSamePeriodTotal { get; set; }
    public decimal DifferenceAmount { get; set; }
    public decimal PercentageChange { get; set; }
}

public class MerchantDetailResponseViewModel
{
    public MerchantSummaryViewModel MerchantSummary { get; set; } = new();
    public TransactionListViewModel Transactions { get; set; } = new();
}

public class MerchantSummaryViewModel
{
    public string MerchantId { get; set; } = string.Empty;
    public string MerchantName { get; set; } = string.Empty;
    public string StartDate { get; set; } = string.Empty;
    public string EndDate { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public MerchantComparisonViewModel? Comparison { get; set; }
}

public class MerchantComparisonViewModel
{
    public decimal PreviousMonthSamePeriodTotal { get; set; }
    public decimal DifferenceAmount { get; set; }
    public decimal PercentageChange { get; set; }
    public string Trend { get; set; } = string.Empty;
    public bool IsNewSpending { get; set; }
}

public class TransactionListViewModel
{
    public List<TransactionItemViewModel> Items { get; set; } = new();
    public long Total { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

public class TransactionItemViewModel
{
    public string TransactionId { get; set; } = string.Empty;
    public string TransactionName { get; set; } = string.Empty;
    public string Date { get; set; } = string.Empty;
    public string MerchantName { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}
