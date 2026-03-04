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
            if (request.Page.HasValue && request.Page <= 0)
            {
                context.AddFailure(MessageCodes.InvalidPage);
            }
            if (request.PageSize.HasValue && request.PageSize <= 0)
            {
                context.AddFailure(MessageCodes.InvalidPageSize);
            }
            if (!string.IsNullOrWhiteSpace(request.AccountId) && !ObjectId.TryParse(request.AccountId, out _))
            {
                context.AddFailure(MessageCodes.InvalidAccountId);
            }
        });
    }
}

