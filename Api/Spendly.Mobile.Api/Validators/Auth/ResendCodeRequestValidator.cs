namespace Spendly.Mobile.Api.Validators.Auth;

using FluentValidation;
using Spendly.Mobile.ViewModels.Auth;
using Spendly.Shared.Localization;

public class ResendCodeRequestValidator : AbstractValidator<ResendCodeRequestViewModel>
{
    public ResendCodeRequestValidator()
    {
        RuleFor(x => x.UserId).NotEmpty().WithMessage(MessageCodes.UserNotFound);
    }
}

