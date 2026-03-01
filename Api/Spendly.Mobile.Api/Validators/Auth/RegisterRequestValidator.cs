namespace Spendly.Mobile.Api.Validators.Auth;

using FluentValidation;
using Spendly.Mobile.ViewModels.Auth;
using Spendly.Shared.Localization;

public class RegisterRequestValidator : AbstractValidator<RegisterRequestViewModel>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage(MessageCodes.NameRequired);
        RuleFor(x => x.Surname).NotEmpty().WithMessage(MessageCodes.SurnameRequired);
        RuleFor(x => x.Email).NotEmpty().WithMessage(MessageCodes.EmailRequired);
        RuleFor(x => x.Email).EmailAddress().WithMessage(MessageCodes.EmailInvalid);
        RuleFor(x => x.Password).NotEmpty().WithMessage(MessageCodes.PasswordRequired);
    }
}

