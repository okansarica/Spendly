// CHANGED_BY_AI: 2026-03-02 - Add report category validation
namespace Spendly.Mobile.Api.Validators.Reports;

using FluentValidation;
using MongoDB.Bson;
using Spendly.Mobile.ViewModels.Reports;
using Spendly.Shared.Localization;

public class ReportsCategoryRequestValidator : AbstractValidator<ReportsCategoryRequestViewModel>
{
    public ReportsCategoryRequestValidator()
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
            if (!string.IsNullOrWhiteSpace(request.SortBy) && request.SortBy != "date" && request.SortBy != "amount")
            {
                context.AddFailure(MessageCodes.InvalidSortBy);
            }
            if (!string.IsNullOrWhiteSpace(request.SortDirection) && request.SortDirection != "asc" && request.SortDirection != "desc")
            {
                context.AddFailure(MessageCodes.InvalidSortDirection);
            }
            if (request.Page.HasValue && request.Page <= 0)
            {
                context.AddFailure(MessageCodes.InvalidPage);
            }
            if (request.PageSize.HasValue && request.PageSize <= 0)
            {
                context.AddFailure(MessageCodes.InvalidPageSize);
            }
            if (request.AccountIds != null && request.AccountIds.Any(id => !ObjectId.TryParse(id, out _)))
            {
                context.AddFailure(MessageCodes.InvalidAccountId);
            }
        });
    }
}

