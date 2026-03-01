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
}

