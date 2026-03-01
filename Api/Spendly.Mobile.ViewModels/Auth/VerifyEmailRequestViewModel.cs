namespace Spendly.Mobile.ViewModels.Auth;

public class VerifyEmailRequestViewModel
{
    public string UserId { get; set; } = null!;
    public string Code { get; set; } = null!;
}

