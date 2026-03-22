// CHANGED_BY_AI: 2026-03-12 - Add optional payment url for post-verification paid registration flow
namespace Spendly.Mobile.ViewModels.Auth;

using Shared.Enums;

public class AuthResponseViewModel
{
    public required string? Id { get; set; }
    public required string? Email { get; set; }
    public string? AccessToken { get; set; }
    public DateTime? AccessTokenExpire { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpire { get; set; }
    public required bool EmailVerificationRequired { get; set; }
    public required string LanguageCode { get; set; }
    public string? PaymentUrl { get; set; }
    public SubscriptionType SubscriptionType { get; set; }
}

