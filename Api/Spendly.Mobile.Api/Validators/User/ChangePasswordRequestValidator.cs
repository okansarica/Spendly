// CHANGED_BY_AI: 2026-03-03 - Add change password validator
namespace Spendly.Mobile.Api.Validators.User;

using FluentValidation;
using Spendly.Mobile.ViewModels.User;
using Spendly.Shared.Localization;

public class ChangePasswordRequestValidator : AbstractValidator<ChangePasswordRequestViewModel>
{
    public ChangePasswordRequestValidator()
    {
        RuleFor(x => x.CurrentPassword).NotEmpty().WithMessage(MessageCodes.PasswordRequired);
        RuleFor(x => x.NewPassword).NotEmpty().WithMessage(MessageCodes.PasswordRequired);
        RuleFor(x => x.ConfirmNewPassword).NotEmpty().WithMessage(MessageCodes.PasswordRequired);
    }
}

