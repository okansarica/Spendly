namespace Spendly.Shared.Entities.Banking;

using MongoDB.Bson;
using Spendly.Shared.Entities.Core;

/// <summary>
/// Each bank connection should have separate token. Bank entity has a FK to this table
/// </summary>
public class UserPlaidToken : BaseEntity
{
    public required ObjectId UserId { get; set; }

    public string EncryptedAccessToken { get; set; } = string.Empty;

    /// <summary>
    /// Plaid’in oluşturduğu Item’ın benzersiz kimliğidir.
    /// Plaid’de Item = kullanıcının bağladığı banka bağlantısıdır.
    /// </summary>
    public string ItemId { get; set; } = string.Empty;

    public DateTime? ExpirationDateTime { get; set; }
}

