namespace Spendly.Shared.Entities.Reporting;

using Core;
using MongoDB.Bson;

public class DailyCategoryAccountExpense : BaseReportEntity
{
    public required ObjectId BankId { get; set; }
    public required DateOnly Date { get; set; }
    public required ObjectId UserCategoryId { get; set; }
    public required ObjectId AccountId { get; set; }
    public required decimal TotalAmount { get; set; }
    public required decimal TotalCount { get; set; }
}

