namespace Spendly.Shared.Entities.TransactionManagement;

using Core;
using MongoDB.Bson;

public class NormalizedTransaction : BaseEntity
{
    public ObjectId RawTransactionId { get; set; }
    public ObjectId UserId { get; set; }
    public ObjectId AccountId { get; set; }
    
    /// <summary>
    /// Belirlenemezse other category kullnilir
    /// </summary>
    public ObjectId UserCategoryId { get; set; } 
    
    /// <summary>
    /// Belirlenemezse other merchant kullanilir
    /// </summary>
    public ObjectId MerchantId { get; set; }
    
    /// <summary>
    /// Belirlenemezse User other merchant kulanilir
    /// </summary>
    public ObjectId UserMerchantId { get; set; }
    
    public string PlaidTransactionId { get; set; } = string.Empty;
    public DateTime DateTime { get; set; }
    public decimal Amount { get; set; }
    public string TransactionName { get; set; } = string.Empty;
}

