namespace Spendly.Shared.Entities.Reporting;

using Core;
using MongoDB.Bson;

public class CategoryMonthlyExpense : BaseEntity
{
    public ObjectId CategoryId { get; set; }
    public int TransactionCount { get; set; }
    public decimal TransactionAmount { get; set; }
    public int Year { get; set; }
    public int Month { get; set; }
}

