// CHANGED_BY_AI: 2026-03-03 - Add language preference validator
namespace Spendly.Mobile.Api.Validators.User;

using FluentValidation;
using Spendly.Mobile.ViewModels.User;
using Spendly.Shared.Localization;

public class SetLanguagePreferenceRequestValidator : AbstractValidator<SetLanguagePreferenceRequestViewModel>
{
    public SetLanguagePreferenceRequestValidator()
    {
        RuleFor(x => x.LanguageCode).NotEmpty().WithMessage(MessageCodes.InvalidLanguage);
    }
}

