// CHANGED_BY_AI: 2026-03-12 - Add selected subscription plan to register request
namespace Spendly.Mobile.ViewModels.Auth;

using Spendly.Shared.Enums;

public class RegisterRequestViewModel
{
    public string Name { get; set; } = null!;
    public string Surname { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
    public SubscriptionType SubscriptionType { get; set; }
    public UserSubscriptionDurationType? Duration { get; set; }
    public string FirebaseToken { get; set; } = null!;
}


