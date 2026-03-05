namespace Spendly.Mobile.ViewModels.User;

using Spendly.Shared.Enums;

public class SubscriptionPlanResponseViewModel
{
	public UserSubscriptionDurationType PlanType { get; set; }
	public decimal Price { get; set; }
}

public class CreatePaymentUrlRequestViewModel
{
	public UserSubscriptionDurationType SelectedPlanType { get; set; }
}

public class CreatePaymentUrlResponseViewModel
{
	public string PaymentUrl { get; set; } = string.Empty;
}

