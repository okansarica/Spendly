namespace Spendly.Shared.Entities.Reporting;

using Core;
using MongoDB.Bson;

public class CategoryMonthlyExpense : BaseReportEntity
{
    public ObjectId UserCategoryId { get; set; }
    public int Year { get; set; }
    public int Month { get; set; }
    public decimal TotalAmount { get; set; }
    public required decimal TotalCount { get; set; }
}

