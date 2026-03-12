namespace Spendly.Mobile.Api.Infrastructure;

//kullanicinin subscriptioni yoksa 400 doner tum endpointler. bu nedenle bu attribute sayesinde ignore olur
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public class SkipSubscriptionCheckAttribute : Attribute
{
}

