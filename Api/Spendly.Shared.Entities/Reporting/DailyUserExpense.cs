namespace Spendly.Shared.Entities.Reporting;

using Core;
using MongoDB.Bson;

public class DailyUserExpense : BaseReportEntity
{
    public ObjectId UserId { get; set; }
    public DateTime DateTime { get; set; }
    public decimal TotalAmount { get; set; }
}

