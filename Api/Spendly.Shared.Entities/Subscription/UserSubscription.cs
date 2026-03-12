namespace Spendly.Shared.Entities.Subscription;

using Core;
using Enums;
using MongoDB.Bson;

/// <summary>
/// User can have only active subscription
/// </summary>
public class UserSubscription:BaseEntity
{
	public ObjectId UserId { get; set; }
	public DateTime? StartDateTime { get; set; }

	/// <summary>
	/// Expected means calculated end datetime
	/// </summary>
	public DateTime ExpectedEndDateTime { get; set; }

	/// <summary>
	/// This will be populated in case ended before expected
	/// </summary>
	public DateTime? EndDateTime { get; set; }
	public UserSubscriptionPayment Payment { get; set; } = new ();
	public SubscriptionType SubscriptionType { get; set; }
}

public class UserSubscriptionPayment
{
	public UserSubscriptionDurationType Duration { get; set; }
	public decimal Amount { get; set; }
	public UserSubscriptionPaymentStatusType PaymentStatus { get; set; }
}


public class UserSubscriptionPaymentUrl : BaseEntity
{
	public ObjectId UserSubscriptionId { get; set; }
	public string StripeSessionId { get; set; } = string.Empty;
	public string PaymentUrl { get; set; } = string.Empty;
}

public class StripeCommunicationLog : BaseReportEntity
{
	public string ClientReferenceId { get; set; } = string.Empty;
	public string RequestPayload { get; set; } = string.Empty;
	public string ResponsePayload { get; set; } = string.Empty;
	public string Headers { get; set; } = string.Empty;
	
}
