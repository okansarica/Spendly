// CHANGED_BY_AI: 2026-03-02 - Add reports request view models
namespace Spendly.Mobile.ViewModels.Reports;

public class ReportsOverviewRequestViewModel
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? Timezone { get; set; }
}

public class ReportsCategoryRequestViewModel
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? AccountId { get; set; }
    public int? Page { get; set; }
    public int? PageSize { get; set; }
    public string? Timezone { get; set; }
}

public class AccountsOverviewRequestViewModel
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? Timezone { get; set; }
}

public class AccountDetailRequestViewModel
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? Timezone { get; set; }
}

