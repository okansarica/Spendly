namespace Spendly.Shared.Entities.Reporting;

using Core;
using MongoDB.Bson;

public class MonthlyUserExpense : BaseReportEntity
{
    public ObjectId UserId { get; set; }
    public int Year { get; set; }
    public int Month { get; set; }
    public decimal TotalAmount { get; set; }
}

