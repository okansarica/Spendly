namespace Spendly.Shared.Entities.Reporting;

using Core;
using MongoDB.Bson;

public class MonthlyUserExpense : BaseReportEntity
{
    public required int Year { get; set; }
    public required int Month { get; set; }
    public required decimal TotalAmount { get; set; }
    public required decimal TotalCount { get; set; }
}

