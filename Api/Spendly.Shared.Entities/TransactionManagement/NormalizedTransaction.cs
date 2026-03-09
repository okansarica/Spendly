namespace Spendly.Shared.Entities.TransactionManagement;

using Core;
using MongoDB.Bson;

public class NormalizedTransaction : BaseEntity
{
    public ObjectId RawTransactionId { get; set; }
    public ObjectId UserId { get; set; }
    public ObjectId AccountId { get; set; }
    public ObjectId? UserCategoryId { get; set; } //TODO neden ihtiyac var zaten merchantta category var
    
    /// <summary>
    /// Merchant id can be null when
    /// Transfer, deposit, withdrawal
    ///  Manual transactions / corrections
    /// Investment veya loan account transaction
    /// Unrecognized / unknown merchant
    /// </summary>
    public ObjectId? MerchantId { get; set; }
    
    //TODO user mercahtn id ye ihtiyac var mi?

    public string PlaidTransactionId { get; set; } = string.Empty;
    public DateTime DateTime { get; set; }
    public decimal Amount { get; set; }
    public string TransactionName { get; set; } = string.Empty;
}

