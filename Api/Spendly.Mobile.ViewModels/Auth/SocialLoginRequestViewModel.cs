namespace Spendly.Mobile.ViewModels.Auth;

using Spendly.Shared.Enums;

public class SocialLoginRequestViewModel
{
    public LoginProviderType Provider { get; set; }
    public string Token { get; set; } = null!;
    public string FirebaseToken { get; set; } = null!;
}
