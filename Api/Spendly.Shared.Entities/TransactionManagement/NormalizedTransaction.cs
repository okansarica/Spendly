namespace Spendly.Shared.Entities.TransactionManagement;

using Core;
using MongoDB.Bson;

public class NormalizedTransaction : BaseEntity
{
    public ObjectId RawTransactionId { get; set; }
    public ObjectId UserId { get; set; }
    public ObjectId AccountId { get; set; }
    public ObjectId? CategoryId { get; set; }
    
    /// <summary>
    /// Merchant id can be null when
    /// Transfer, deposit, withdrawal
    ///  Manual transactions / corrections
    /// Investment veya loan account transaction
    /// Unrecognized / unknown merchant
    /// </summary>
    public ObjectId? MerchantId { get; set; }

    public string PlaidTransactionId { get; set; } = string.Empty;
    public DateTime DateTime { get; set; }
    public decimal Amount { get; set; }
}

