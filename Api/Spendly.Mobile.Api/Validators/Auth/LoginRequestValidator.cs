namespace Spendly.Mobile.Api.Validators.Auth;

using FluentValidation;
using Spendly.Mobile.ViewModels.Auth;

public class LoginRequestValidator : AbstractValidator<LoginRequestViewModel>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty();
    }
}

