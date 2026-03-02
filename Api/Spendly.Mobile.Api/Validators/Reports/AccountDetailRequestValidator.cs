// CHANGED_BY_AI: 2026-03-02 - Add account detail validation
namespace Spendly.Mobile.Api.Validators.Reports;

using FluentValidation;
using Spendly.Mobile.ViewModels.Reports;
using Spendly.Shared.Localization;

public class AccountDetailRequestValidator : AbstractValidator<AccountDetailRequestViewModel>
{
    public AccountDetailRequestValidator()
    {
        RuleFor(x => x).Custom((request, context) =>
        {
            if (request.StartDate.HasValue && request.EndDate.HasValue && request.StartDate > request.EndDate)
            {
                context.AddFailure(MessageCodes.InvalidDateRange);
            }
            if (!string.IsNullOrWhiteSpace(request.Timezone) && !TimeZoneInfo.TryFindSystemTimeZoneById(request.Timezone, out _))
            {
                context.AddFailure(MessageCodes.InvalidTimezone);
            }
        });
    }
}

