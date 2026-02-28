namespace Spendly.Mobile.Api.Validators.Auth;

using FluentValidation;
using Spendly.Mobile.ViewModels.Auth;

public class SocialLoginRequestValidator : AbstractValidator<SocialLoginRequestViewModel>
{
    public SocialLoginRequestValidator()
    {
        RuleFor(x => x.Provider).NotEmpty().Must(p => p == "google" || p == "facebook")
            .WithMessage("Provider must be 'google' or 'facebook'.");
        RuleFor(x => x.Token).NotEmpty();
    }
}

