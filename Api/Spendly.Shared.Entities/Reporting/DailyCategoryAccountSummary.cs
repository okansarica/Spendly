namespace Spendly.Shared.Entities.Reporting;

using Core;
using MongoDB.Bson;

public class DailyCategoryAccountExpense : BaseReportEntity
{
    public ObjectId UserId { get; set; }
    public DateTime Date { get; set; }
    public ObjectId BankId { get; set; }
    public ObjectId CategoryId { get; set; }
    public ObjectId AccountId { get; set; }
    public decimal TotalAmount { get; set; }
}

