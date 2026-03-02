// CHANGED_BY_AI: 2026-03-02 - Add reports overview response view models
namespace Spendly.Mobile.ViewModels.Reports;

public class ReportsOverviewResponseViewModel
{
    public ReportSummaryViewModel Summary { get; set; } = new();
    public List<ReportCategoryChangeViewModel> TopChangingCategories { get; set; } = new();
    public List<ReportCategoryDistributionViewModel> CategoryDistribution { get; set; } = new();
    public List<ReportCategoryChangeViewModel> Categories { get; set; } = new();
}

public class ReportSummaryViewModel
{
    public decimal CurrentMonthToDateTotal { get; set; }
    public decimal PreviousMonthSamePeriodTotal { get; set; }
    public decimal DifferenceAmount { get; set; }
    public decimal PercentageChange { get; set; }
    public string Trend { get; set; } = string.Empty;
    public bool IsNewSpending { get; set; }
}

public class ReportCategoryChangeViewModel
{
    public string CategoryId { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public decimal CurrentMonthToDateTotal { get; set; }
    public decimal PreviousMonthSamePeriodTotal { get; set; }
    public decimal DifferenceAmount { get; set; }
    public decimal PercentageChange { get; set; }
}

public class ReportCategoryDistributionViewModel
{
    public string CategoryId { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public decimal CurrentMonthToDateTotal { get; set; }
    public decimal PercentageOfTotal { get; set; }
}

