// CHANGED_BY_AI: 2026-03-04 - Add subscription and Stripe payment service
namespace Spendly.Mobile.BusinessLayer.Services.User;

using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using Shared.Core;
using Shared.Entities.UserManagement;
using Spendly.Mobile.BusinessLayer.Services.Notification;
using Spendly.Mobile.ViewModels.User;
using Spendly.Shared.DataLayer;
using Spendly.Shared.Entities.Subscription;
using Spendly.Shared.Enums;
using Spendly.Shared.ViewModels;
using Spendly.Shared.ViewModels.Settings;
using Stripe;
using Stripe.Checkout;

public class SubscriptionService(
	IRepository<UserSubscription> userSubscriptionRepository,
	IRepository<UserSubscriptionPaymentUrl> paymentUrlRepository,
	IRepository<StripeCommunicationLog> stripeCommunicationLogRepository,
	IRepository<FirebaseToken> firebaseTokenRepository,
	RequestContextViewModel requestContextViewModel,
	StripeSettings stripeSettings,
	FirebaseNotificationService firebaseNotificationService,
	ILogger<SubscriptionService> logger)
{
	private const decimal MonthlyPrice = 3.99m;
	private const decimal YearlyPrice = 39.99m;

	public async Task<FunctionResponse<List<SubscriptionPlanResponseViewModel>>> GetSubscriptionPlansAsync()
	{
		var plans = new List<SubscriptionPlanResponseViewModel>
		{
			new() {PlanType = UserSubscriptionDurationType.Monthly, Price = MonthlyPrice},
			new() {PlanType = UserSubscriptionDurationType.Yearly, Price = YearlyPrice}
		};

		return FunctionResponse.Success(plans);
	}

	public async Task<FunctionResponse<CreatePaymentUrlResponseViewModel>> CreatePaymentUrlAsync(CreatePaymentUrlRequestViewModel request)
	{
		var userId = requestContextViewModel.UserId.ToObjectId();
		var amount = request.SelectedPlanType == UserSubscriptionDurationType.Monthly ? MonthlyPrice : YearlyPrice;

		var userSubscription = await userSubscriptionRepository.GetRequiredAsync(p => p.UserId == userId);

		var clientReferenceId = ObjectId.GenerateNewId().ToString();

		StripeConfiguration.ApiKey = stripeSettings.ApiKey;

		var subscriptionName = request.SelectedPlanType == UserSubscriptionDurationType.Monthly ? "Monthly" : "Yearly";

		var options = new SessionCreateOptions
		{
			PaymentMethodTypes = ["card"],
			LineItems =
			[
				new()
				{
					PriceData = new SessionLineItemPriceDataOptions
					{
						Currency = "gbp",
						ProductData = new SessionLineItemPriceDataProductDataOptions
						{
							Name = $"Spendly {subscriptionName} Subscription"
						},
						UnitAmount = (long) (amount * 100)
					},
					Quantity = 1
				}
			],
			Mode = "payment",
			SuccessUrl = stripeSettings.SuccessUrl,
			CancelUrl = stripeSettings.CancelUrl,
			ClientReferenceId = clientReferenceId,
			PaymentIntentData = new SessionPaymentIntentDataOptions
			{
				Metadata = new Dictionary<string, string>
				{
					["client_reference_id"] = clientReferenceId,
				}
			}
		};

		var requestPayload = System.Text.Json.JsonSerializer.Serialize(options);

		var stripeService = new SessionService();

		Session? session;
		try
		{
			session = await stripeService.CreateAsync(options);
		}
		catch (Exception ex)
		{
			logger.LogError(ex,
				"Stripe payment session creation failed. UserId: {UserId}, PlanType: {PlanType}, ClientReferenceId: {ClientReferenceId}",
				userId,
				request.SelectedPlanType,
				clientReferenceId);
			return FunctionResponse<CreatePaymentUrlResponseViewModel>.Failure("STRIPE_ERROR");
		}

		var responsePayload = System.Text.Json.JsonSerializer.Serialize(session);

		await stripeCommunicationLogRepository.InsertAsync(new StripeCommunicationLog
		{
			ClientReferenceId = clientReferenceId,
			RequestPayload = requestPayload,
			ResponsePayload = responsePayload,
			Headers = string.Empty,
		});

		var paymentUrl = new UserSubscriptionPaymentUrl
		{
			Id = clientReferenceId.ToObjectId(),
			UserSubscriptionId = userSubscription.Id,
			StripeSessionId = session.Id,
			PaymentUrl = session.Url,
		};

		await paymentUrlRepository.InsertAsync(paymentUrl);

		return FunctionResponse.Success(new CreatePaymentUrlResponseViewModel
		{
			PaymentUrl = session.Url
		});
	}

	public async Task<FunctionResponse> HandleStripeWebhookAsync(string payload, string signature)
	{
		var log = new StripeCommunicationLog
		{
			ClientReferenceId = string.Empty,
			RequestPayload = payload,
			ResponsePayload = string.Empty,
			Headers = signature,
		};
		
		await stripeCommunicationLogRepository.InsertAsync(log);

		try
		{
			var stripeEvent = EventUtility.ConstructEvent(
				payload,
				signature,
				stripeSettings.WebhookSecret
			);

			log.ResponsePayload = System.Text.Json.JsonSerializer.Serialize(stripeEvent.Data.Object);
			await stripeCommunicationLogRepository.UpdateAsync(log);
			
			if (stripeEvent.Type != "payment_intent.succeeded" && stripeEvent.Type != "payment_intent.payment_failed" && stripeEvent.Type != "payment_intent.failed" && stripeEvent.Type != "checkout.session.completed" && stripeEvent.Type != "checkout.session.async_payment_failed" && stripeEvent.Type != "checkout.session.expired")
			{
				logger.LogInformation($"Ignored the type: {stripeEvent.Type}");
				return FunctionResponse.Success();
			}

			string? clientReferenceId = null;
			string? paymentIntentId = null;

			if (stripeEvent.Data.Object is PaymentIntent paymentIntent)
			{
				paymentIntentId = paymentIntent.Id;
				if (paymentIntent.Metadata != null && paymentIntent.Metadata.TryGetValue("client_reference_id", out var metadataReferenceId))
				{
					clientReferenceId = metadataReferenceId;
				}
			}
			else if (stripeEvent.Data.Object is Charge charge)
			{
				paymentIntentId = charge.PaymentIntentId;
			}
			else if (stripeEvent.Data.Object is Session session)
			{
				clientReferenceId = session.ClientReferenceId;
				paymentIntentId = session.PaymentIntentId;
			}

			if (string.IsNullOrWhiteSpace(clientReferenceId) && !string.IsNullOrWhiteSpace(paymentIntentId))
			{
				var stripeSessionService = new SessionService();
				var sessions = await stripeSessionService.ListAsync(new SessionListOptions
				{
					PaymentIntent = paymentIntentId,
					Limit = 1,
				});
				clientReferenceId = sessions.Data.FirstOrDefault()?.ClientReferenceId;
			}

			if (string.IsNullOrWhiteSpace(clientReferenceId))
			{
				logger.LogWarning("Stripe webhook received without ClientReferenceId");
				throw new Exception("CLIENTREFERENCE_ID_NOT_FOUND");
			}

			log.ClientReferenceId = clientReferenceId;
			await stripeCommunicationLogRepository.UpdateAsync(log);
			
			var paymentUrl = await paymentUrlRepository.GetAsync(x => x.Id == clientReferenceId.ToObjectId());
			if (paymentUrl == null)
			{
				logger.LogWarning("Payment URL not found for ClientReferenceId: {ClientReferenceId}", clientReferenceId);
				return FunctionResponse.Failure("WEBHOOK_PAYMENT_URL_NOT_FOUND");
			}
			
			var subscription = await userSubscriptionRepository.GetRequiredAsync(paymentUrl.UserSubscriptionId);
			var firebaseToken = await firebaseTokenRepository.GetRequiredAsync(p => p.UserId == subscription.UserId);
			
			if (stripeEvent.Type == "payment_intent.succeeded" || stripeEvent.Type == "checkout.session.completed")
			{
				subscription.Payment.PaymentStatus = UserSubscriptionPaymentStatusType.Paid;
				subscription.StartDateTime = DateTime.UtcNow;
				subscription.ExpectedEndDateTime = subscription.Payment.Duration == UserSubscriptionDurationType.Monthly ? DateTime.UtcNow.AddMonths(1) : DateTime.UtcNow.AddYears(1);
			
				await userSubscriptionRepository.UpdateAsync(subscription);
			
				logger.LogInformation("Subscription updated successfully. SubscriptionId: {SubscriptionId}, ClientReferenceId: {ClientReferenceId}",
					subscription.Id,
					clientReferenceId);
				
				await firebaseNotificationService.SendSubscriptionPaymentResultAsync(firebaseToken.Token, SubscriptionPaymentResultStatusType.Success);
			
				return FunctionResponse.Success();
			}
			
			subscription.Payment.PaymentStatus = UserSubscriptionPaymentStatusType.Failed;
			await userSubscriptionRepository.UpdateAsync(subscription);
			
			await firebaseNotificationService.SendSubscriptionPaymentResultAsync(firebaseToken.Token, SubscriptionPaymentResultStatusType.Fail);
			return FunctionResponse.Success();
		}
		catch (StripeException ex)
		{
			logger.LogError(ex, "Stripe webhook signature verification failed");
			return FunctionResponse.Failure("WEBHOOK_VERIFICATION_FAILED");
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "Stripe webhook processing failed");
			return FunctionResponse.Failure("WEBHOOK_PROCESSING_FAILED");
		}
	}
	
	public async Task<FunctionResponse> SaveFirebaseToken(SaveFirebaseTokenRequest request)
	{
		var userId = requestContextViewModel.TryToGetUserId();
		
		var firebaseToken = await firebaseTokenRepository.GetAsync(p => p.Token == request.Token);
		if (firebaseToken == null)
		{
			firebaseToken = new FirebaseToken
			{
				Token = request.Token,
				UserId = userId?.ToObjectId()
			};
			await firebaseTokenRepository.InsertAsync(firebaseToken);
			return FunctionResponse.Success();
		}

		if (userId != null)
		{
			firebaseToken.UserId = userId.ToObjectId();
			await firebaseTokenRepository.UpdateAsync(firebaseToken);
		}
		
		return FunctionResponse.Success();
	}
}
