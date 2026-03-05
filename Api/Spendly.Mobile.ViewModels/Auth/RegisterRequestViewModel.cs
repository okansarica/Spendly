namespace Spendly.Mobile.ViewModels.Auth;

public class RegisterRequestViewModel
{
    public string Name { get; set; } = null!;
    public string Surname { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string FirebaseToken { get; set; } = null!;
}


