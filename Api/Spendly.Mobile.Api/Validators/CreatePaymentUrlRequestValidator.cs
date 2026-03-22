namespace Spendly.Mobile.Api.Validators;

using FluentValidation;
using ViewModels.User;

public class CreatePaymentUrlRequestValidator : AbstractValidator<CreatePaymentUrlRequestViewModel>
{
	public CreatePaymentUrlRequestValidator()
	{
		RuleFor(x => x.DurationType).IsInEnum();
	}
}

