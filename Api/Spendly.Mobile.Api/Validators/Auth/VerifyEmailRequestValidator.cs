namespace Spendly.Mobile.Api.Validators.Auth;

using FluentValidation;
using Spendly.Mobile.ViewModels.Auth;
using Spendly.Shared.Localization;

public class VerifyEmailRequestValidator : AbstractValidator<VerifyEmailRequestViewModel>
{
    public VerifyEmailRequestValidator()
    {
        RuleFor(x => x.UserId).NotEmpty().WithMessage(MessageCodes.UserNotFound);
        RuleFor(x => x.Code).NotEmpty().WithMessage(MessageCodes.InvalidVerificationCode);
    }
}

