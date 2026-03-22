namespace Spendly.Mobile.ViewModels.User;

using Spendly.Shared.Enums;

public class SubscriptionPlanResponseViewModel
{
	public SubscriptionType SubscriptionType { get; set; }
	public UserSubscriptionDurationType? DurationType { get; set; }
	public decimal Price { get; set; }
}

public class CreatePaymentUrlRequestViewModel
{
	public SubscriptionType SubscriptionType { get; set; }
	public UserSubscriptionDurationType DurationType { get; set; }
}

public class CreatePaymentUrlWithTokenRequestViewModel
{
	public SubscriptionType SubscriptionType { get; set; }
	public string AccessToken { get; set; } = string.Empty;
	public UserSubscriptionDurationType DurationType { get; set; }
}

public class CreatePaymentUrlResponseViewModel
{
	public string PaymentUrl { get; set; } = string.Empty;
}

