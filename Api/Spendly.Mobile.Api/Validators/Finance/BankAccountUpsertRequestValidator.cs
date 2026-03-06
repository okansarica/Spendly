// CHANGED_BY_AI: 2026-03-06 - Add bank account upsert request validator
namespace Spendly.Mobile.Api.Validators.Finance;

using FluentValidation;
using Spendly.Mobile.ViewModels.Finance;
using Spendly.Shared.Localization;

public class BankAccountUpsertRequestValidator : AbstractValidator<BankAccountUpsertRequestViewModel>
{
    public BankAccountUpsertRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage(MessageCodes.AccountNameRequired);
    }
}

