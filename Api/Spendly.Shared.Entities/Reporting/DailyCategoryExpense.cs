namespace Spendly.Shared.Entities.Reporting;

using Core;
using MongoDB.Bson;

public class DailyCategoryExpense : BaseEntity
{
    public ObjectId UserId { get; set; }
    public ObjectId CategoryId { get; set; }
    public DateTime DateTime { get; set; }
    public decimal TotalAmount { get; set; }
}

