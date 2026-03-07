namespace Spendly.Shared.Entities.Banking;

using Spendly.Shared.Entities.Core;

public class UserPlaidToken : BaseEntity
{
    public string UserId { get; set; } = string.Empty;

    public string EncryptedAccessToken { get; set; } = string.Empty;

    public string ItemId { get; set; } = string.Empty;

    public DateTime? ExpirationDateTime { get; set; }
}

