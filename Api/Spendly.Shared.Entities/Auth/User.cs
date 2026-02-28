namespace Spendly.Shared.Entities.Auth;

using MongoDB.Bson.Serialization.Attributes;
using Spendly.Shared.Entities.Core;
using Spendly.Shared.Enums;

public class User : BaseEntity
{
    public string Email { get; set; } = null!;
    public string? PasswordHash { get; set; }
    public EmailVerification EmailVerification { get; set; } = new();
    public List<UserLoginProvider> LoginProviders { get; set; } = new();
    public bool IsActive { get; set; } = true;
    public string? RefreshToken { get; set; }

    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime? RefreshTokenExpiry { get; set; }
}

public class EmailVerification
{
    public bool IsVerified { get; set; }
    public string? VerificationCode { get; set; }

    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime? LastCodeIssuedAt { get; set; }

    public int VerificationAttemptCount { get; set; }
    public bool IsVerificationLocked { get; set; }
}

public class UserLoginProvider
{
    public LoginProviderType Provider { get; set; }
    public string? ProviderUserId { get; set; }
}

