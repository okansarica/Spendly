// CHANGED_BY_AI: 2026-03-02 - Add category upsert validation
namespace Spendly.Mobile.Api.Validators.Finance;

using FluentValidation;
using MongoDB.Bson;
using Spendly.Mobile.ViewModels.Finance;
using Spendly.Shared.Localization;

public class CategoryUpsertRequestValidator : AbstractValidator<CategoryUpsertRequestViewModel>
{
    public CategoryUpsertRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage(MessageCodes.CategoryNameRequired);

        RuleFor(x => x).Custom((request, context) =>
        {
            if (!string.IsNullOrWhiteSpace(request.ParentId) && !ObjectId.TryParse(request.ParentId, out _))
            {
                context.AddFailure(MessageCodes.InvalidCategoryId);
            }

            if (request.MerchantIds != null && request.MerchantIds.Any(id => !ObjectId.TryParse(id, out _)))
            {
                context.AddFailure(MessageCodes.MerchantNotFound);
            }
        });
    }
}

