// CHANGED_BY_AI: 2026-03-06 - Add bank upsert request validator
namespace Spendly.Mobile.Api.Validators.Finance;

using FluentValidation;
using MongoDB.Bson;
using Spendly.Mobile.ViewModels.Finance;
using Spendly.Shared.Localization;

public class BankUpsertRequestValidator : AbstractValidator<BankUpsertRequestViewModel>
{
    public BankUpsertRequestValidator()
    {
        RuleFor(x => x).Custom((request, context) =>
        {
            if (!string.IsNullOrWhiteSpace(request.BankDefinitionId) && !ObjectId.TryParse(request.BankDefinitionId, out _))
            {
                context.AddFailure(MessageCodes.InvalidBankDefinitionId);
            }

            if (string.IsNullOrWhiteSpace(request.BankDefinitionId) && string.IsNullOrWhiteSpace(request.Name))
            {
                context.AddFailure(MessageCodes.BankNameRequired);
            }
        });
    }
}

