namespace Spendly.Shared.Entities.TransactionManagement;

using Core;
using MongoDB.Bson;

public class Merchant : BaseEntity
{
    public ObjectId CategoryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int TransactionCount { get; set; }
    public decimal TransactionAmount { get; set; }
}

