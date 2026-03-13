namespace Spendly.Shared.Entities.Reporting;

using Core;
using MongoDB.Bson;

public class MerchantMonthlyExpense : BaseReportEntity
{
    public required ObjectId MerchantId { get; set; }
    public required int Year { get; set; }
    public required int Month { get; set; }
    public decimal TotalAmount { get; set; }
    public required decimal TotalCount { get; set; }
}

