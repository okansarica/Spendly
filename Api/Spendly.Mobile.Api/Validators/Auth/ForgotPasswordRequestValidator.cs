namespace Spendly.Mobile.Api.Validators.Auth;

using FluentValidation;
using Spendly.Mobile.ViewModels.Auth;

public class ForgotPasswordRequestValidator : AbstractValidator<ForgotPasswordRequestViewModel>
{
    public ForgotPasswordRequestValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
    }
}

