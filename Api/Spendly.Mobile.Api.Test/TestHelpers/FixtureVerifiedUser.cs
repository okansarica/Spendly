using Spendly.Mobile.ViewModels.Auth;

namespace Spendly.Mobile.Api.Test.TestHelpers;

public class FixtureVerifiedUser
{
    public RegisterRequestViewModel Register { get; set; } = null!;
    public string UserId { get; set; } = string.Empty;
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
}
