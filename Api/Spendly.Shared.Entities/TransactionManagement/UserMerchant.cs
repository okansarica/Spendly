// CHANGED_BY_AI: 2026-03-02 - Add merchant nickname
// CHANGED_BY_AI: 2026-03-02 - Add merchant ownership and uncategorized support

namespace Spendly.Shared.Entities.TransactionManagement;

using Core;
using MongoDB.Bson;


public class Merchant : BaseEntity
{
    public ObjectId CategoryId { get; set; }
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Sadece isim dolu id bos gelebiliyor plaidden
    /// </summary>
    public string? PlaidId { get; set; }
    public string? LogoUrl { get; set; }
    
    /// <summary>
    /// Merchanti olmayan transactionlar buraya baglanir
    /// </summary>
    public bool IsOther { get; set; }
}

/// <summary>
/// Ozellestirilmis ek bilgi
/// </summary>
public class UserMerchant : BaseEntity
{
    public ObjectId UserId { get; set; }
    public ObjectId? UserCategoryId { get; set; }
    public ObjectId MerchantId { get; set; }
    public string? Nickname { get; set; }
    public int TotalTransactionCount { get; set; }
    public decimal TotalTransactionAmount { get; set; }
    public bool IsOther { get; set; }
}

/// <summary>
/// Bu 1 kere insert edilir, transactionlar cekerken merchantlarin kategorilerini atamak amacli kullanilir
/// </summary>
public class PredefinedMerchant:BaseEntity
{
    public ObjectId CategoryId { get; set; }
    public string Name { get; set; }
    public string? PlaidId { get; set; }
}
