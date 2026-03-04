// CHANGED_BY_AI: 2026-03-03 - Add user profile request/response view models
namespace Spendly.Mobile.ViewModels.User;

public class UserProfileResponseViewModel
{
    public string Id { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string Surname { get; set; } = null!;
    public string Email { get; set; } = null!;
    public bool IsNewsletterSubscribed { get; set; }
    public string LanguageCode { get; set; } = "en";
}

public class UpdateUserProfileRequestViewModel
{
    public string Name { get; set; } = null!;
    public string Surname { get; set; } = null!;
    public bool IsNewsletterSubscribed { get; set; }
}

public class ChangePasswordRequestViewModel
{
    public string CurrentPassword { get; set; } = null!;
    public string NewPassword { get; set; } = null!;
    public string ConfirmNewPassword { get; set; } = null!;
}

public class SetLanguagePreferenceRequestViewModel
{
    public string LanguageCode { get; set; } = null!;
}

public class LanguagePreferenceResponseViewModel
{
    public string LanguageCode { get; set; } = "en";
}

