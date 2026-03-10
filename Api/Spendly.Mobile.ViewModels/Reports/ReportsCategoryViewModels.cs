// CHANGED_BY_AI: 2026-03-02 - Add report category detail view models
namespace Spendly.Mobile.ViewModels.Reports;

public class ReportCategoryDetailResponseViewModel
{
    public ReportCategorySummaryViewModel CategorySummary { get; set; } = new();
    public ReportTransactionPageViewModel Transactions { get; set; } = new();
}

public class ReportCategorySummaryViewModel
{
    public string CategoryId { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
}

public class ReportTransactionPageViewModel
{
    public List<ReportTransactionItemViewModel> Items { get; set; } = new();
    public int Total { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

public class ReportTransactionItemViewModel
{
    public string TransactionId { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string MerchantName { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string TransactionName { get; set; }  = string.Empty;
}

