namespace Spendly.Mobile.ViewModels.Auth;

public class SocialLoginRequestViewModel
{
    public string Provider { get; set; } = null!; //TODO enum olmali
    public string Token { get; set; } = null!;
    
    public string FirebaseToken { get; set; } = null!;
}

