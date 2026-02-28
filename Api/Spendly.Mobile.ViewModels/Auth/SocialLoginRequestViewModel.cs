namespace Spendly.Mobile.ViewModels.Auth;

public class SocialLoginRequestViewModel
{
    public string Provider { get; set; } = null!;
    public string Token { get; set; } = null!;
}

