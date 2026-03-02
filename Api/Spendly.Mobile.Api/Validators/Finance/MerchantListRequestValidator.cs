// CHANGED_BY_AI: 2026-03-02 - Add merchant list request validation
namespace Spendly.Mobile.Api.Validators.Finance;

using FluentValidation;
using Spendly.Mobile.BusinessLayer.Constants;
using Spendly.Mobile.ViewModels.Finance;
using Spendly.Shared.Localization;

public class MerchantListRequestValidator : AbstractValidator<MerchantListRequestViewModel>
{
    public MerchantListRequestValidator()
    {
        RuleFor(x => x).Custom((request, context) =>
        {
            if (request.StartDate.HasValue && request.EndDate.HasValue && request.StartDate > request.EndDate)
            {
                context.AddFailure(MessageCodes.InvalidDateRange);
            }

            if (!string.IsNullOrWhiteSpace(request.SortBy) &&
                request.SortBy != Constants.Finance.Sort.Name &&
                request.SortBy != Constants.Finance.Sort.Amount &&
                request.SortBy != Constants.Finance.Sort.Transactions &&
                request.SortBy != Constants.Finance.Sort.Date)
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

