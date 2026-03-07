namespace Spendly.Shared.Entities.TransactionManagement;

using Core;
using MongoDB.Bson;

public class NormalizedTransaction : BaseEntity
{
    public ObjectId RawTransactionId { get; set; }
    public ObjectId UserId { get; set; }
    public ObjectId AccountId { get; set; }
    public ObjectId? CategoryId { get; set; }
    public ObjectId MerchantId { get; set; }
    
    public DateTime Date { get; set; }
    public decimal Amount { get; set; }
}

