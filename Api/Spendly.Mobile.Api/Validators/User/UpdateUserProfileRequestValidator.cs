// CHANGED_BY_AI: 2026-03-03 - Add update profile validator
namespace Spendly.Mobile.Api.Validators.User;

using FluentValidation;
using Spendly.Mobile.ViewModels.User;
using Spendly.Shared.Localization;

public class UpdateUserProfileRequestValidator : AbstractValidator<UpdateUserProfileRequestViewModel>
{
    public UpdateUserProfileRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage(MessageCodes.NameRequired);
        RuleFor(x => x.Surname).NotEmpty().WithMessage(MessageCodes.SurnameRequired);
    }
}

