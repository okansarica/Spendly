namespace Spendly.Mobile.Api.Validators.Auth;

using FluentValidation;
using Spendly.Mobile.ViewModels.Auth;
using Spendly.Shared.Enums;

public class SocialLoginRequestValidator : AbstractValidator<SocialLoginRequestViewModel>
{
    public SocialLoginRequestValidator()
    {
        RuleFor(x => x.Provider).IsInEnum();
        RuleFor(x => x.Token).NotEmpty();
    }
}
