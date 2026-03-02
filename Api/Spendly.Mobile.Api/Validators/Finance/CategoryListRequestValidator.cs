// CHANGED_BY_AI: 2026-03-02 - Add category list request validation
namespace Spendly.Mobile.Api.Validators.Finance;

using FluentValidation;
using Spendly.Mobile.BusinessLayer.Constants;
using Spendly.Mobile.ViewModels.Finance;
using Spendly.Shared.Localization;

public class CategoryListRequestValidator : AbstractValidator<CategoryListRequestViewModel>
{
    public CategoryListRequestValidator()
    {
        RuleFor(x => x).Custom((request, context) =>
        {
            if (!string.IsNullOrWhiteSpace(request.SortBy) && request.SortBy != Constants.Finance.Sort.Name)
            {
                context.AddFailure(MessageCodes.InvalidSortBy);
            }

            if (!string.IsNullOrWhiteSpace(request.SortDirection) &&
                request.SortDirection != Constants.Finance.Sort.Asc &&
                request.SortDirection != Constants.Finance.Sort.Desc)
            {
                context.AddFailure(MessageCodes.InvalidSortDirection);
            }
        });
    }
}

