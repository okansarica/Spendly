namespace Spendly.Mobile.BusinessLayer.Services.Auth;

using Spendly.Mobile.ViewModels.Auth;
using Spendly.Shared.ViewModels;

public interface IAuthService
{
    Task<FunctionResponse<AuthResponseViewModel>> LoginAsync(LoginRequestViewModel request);
    Task<FunctionResponse<AuthResponseViewModel>> SocialLoginAsync(SocialLoginRequestViewModel request);
    Task<FunctionResponse> ForgotPasswordAsync(ForgotPasswordRequestViewModel request);
}

