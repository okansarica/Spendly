namespace Spendly.Mobile.BusinessLayer.Services.User;

using Spendly.Shared.Enums;

public class SubscriptionService
{
	// Dictionary of feature objects (extensible via inheritance)
	private readonly static IReadOnlyDictionary<SubscriptionType, ISubscription> Subscriptions = new Dictionary<SubscriptionType, ISubscription>
	{
		[SubscriptionType.Free] = new FreeSubscription(),
		[SubscriptionType.Plus] = new ProSubscription(),
		[SubscriptionType.Pro] = new ProSubscription()
	};

	public static ISubscription Get(SubscriptionType subscriptionType)
	{
		return Subscriptions[subscriptionType];
	}
}
