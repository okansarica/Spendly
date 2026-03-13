// CHANGED_BY_AI: 2026-03-12 - Add optional payment url for post-verification paid registration flow
namespace Spendly.Mobile.ViewModels.Auth;

public class AuthResponseViewModel
{
    public string? Id { get; set; }
    public string? Email { get; set; }
    public string? AccessToken { get; set; }
    public DateTime? AccessTokenExpire { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpire { get; set; }
    public bool EmailVerificationRequired { get; set; }
    public string LanguageCode { get; set; }
    public DateTime? SubscriptionEndDateTime { get; set; }
    public string? PaymentUrl { get; set; }
    public bool SubscriptionExpired { get; set; }
}

