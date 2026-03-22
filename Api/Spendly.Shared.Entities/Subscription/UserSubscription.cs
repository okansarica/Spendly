namespace Spendly.Shared.Entities.Subscription;

using Core;
using Enums;
using MongoDB.Bson;

/// <summary>
/// User can have only active subscription at time t but multiple with passive ones
/// </summary>
public class UserSubscription:BaseEntity
{
	public ObjectId UserId { get; set; }
	public DateTime? StartDateTime { get; set; }
	
	/// <summary>
	/// Expected means calculated end datetime, null for free subscription
	/// </summary>
	public DateTime? ExpectedEndDateTime { get; set; }
	
	/// <summary>
	/// Can be null in free subscription
	/// </summary>
	public DateTime? EndDateTime { get; set; }
	public SubscriptionType SubscriptionType { get; set; }

	public UserSubscriptionStateType State
	{
		get
		{
			if (SubscriptionType== SubscriptionType.Free)
			{
				return UserSubscriptionStateType.Active;
			}
			return field;
		}
		set;
	}
	
	/// <summary>
	/// Can be null in free subscription
	/// </summary>
	public UserSubscriptionDurationType? Duration { get; set; }
}

public class UserSubscriptionPayment
{
	public DateTime? PaymentCompletionDateTime { get; set; }
	
	public decimal Amount { get; set; }
	public UserSubscriptionPaymentStatusType PaymentStatus { get; set; }
}


public class UserSubscriptionPaymentUrl : BaseEntity
{
	public ObjectId UserSubscriptionId { get; set; }
	public string StripeSessionId { get; set; } = string.Empty;
	public string PaymentUrl { get; set; } = string.Empty;
	
	public UserSubscriptionPayment Payment { get; set; } = new ();
}

public class StripeCommunicationLog : BaseLogEntity
{
	public string ClientReferenceId { get; set; } = string.Empty;
	public string RequestPayload { get; set; } = string.Empty;
	public string ResponsePayload { get; set; } = string.Empty;
	public string Headers { get; set; } = string.Empty;
	
}
