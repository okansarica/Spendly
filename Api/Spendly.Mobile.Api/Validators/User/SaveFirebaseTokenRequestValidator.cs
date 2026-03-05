namespace Spendly.Mobile.Api.Validators.User;

using FluentValidation;
using Shared.Localization;
using ViewModels.User;

public class SaveFirebaseTokenRequestValidator: AbstractValidator<SaveFirebaseTokenRequest>
{
	public SaveFirebaseTokenRequestValidator()
	{
		RuleFor(x => x.Token).NotEmpty().WithMessage(MessageCodes.InvalidToken);
	}
}
