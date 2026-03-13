namespace Spendly.Shared.Entities.Reporting;

using Core;
using MongoDB.Bson;

public class DailyUserExpense : BaseReportEntity
{
    public required DateOnly Date { get; set; }
    public required decimal TotalAmount { get; set; }
    public required decimal TotalCount { get; set; }
}

