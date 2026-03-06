namespace Spendly.Shared.Entities.Reporting;

using Core;
using MongoDB.Bson;

public class DailyAccountExpense : BaseEntity
{
    public ObjectId UserId { get; set; }
    public DateTime DateTime { get; set; }
    public ObjectId BankId { get; set; }
    public ObjectId AccountId { get; set; }
    public decimal TotalAmount { get; set; }
}

