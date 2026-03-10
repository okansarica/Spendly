// CHANGED_BY_AI: 2026-03-02 - Add merchant category update validation
namespace Spendly.Mobile.Api.Validators.Finance;

using FluentValidation;
using MongoDB.Bson;
using Spendly.Mobile.ViewModels.Finance;
using Spendly.Shared.Localization;

public class MerchantCategoryUpdateRequestValidator : AbstractValidator<UserMerchantCategoryUpdateRequestViewModel>
{
    public MerchantCategoryUpdateRequestValidator()
    {
        RuleFor(x => x).Custom((request, context) =>
        {
            if (!string.IsNullOrWhiteSpace(request.CategoryId) && !ObjectId.TryParse(request.CategoryId, out _))
            {
                context.AddFailure(MessageCodes.InvalidCategoryId);
            }
        });
    }
}

