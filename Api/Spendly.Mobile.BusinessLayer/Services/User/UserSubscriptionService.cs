// CHANGED_BY_AI: 2026-03-13 - Add token-based payment URL creation for login expired subscription flow
// CHANGED_BY_AI: 2026-03-12 - Reuse payment url creation during registration flow
// CHANGED_BY_AI: 2026-03-04 - Add subscription and Stripe payment service
namespace Spendly.Mobile.BusinessLayer.Services.User;

using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Bson;
using Shared.BusinessLayer;
using Shared.BusinessLayer.Notification;
using Shared.Core.Extensions;
using Shared.Entities.UserManagement;
using Spendly.Mobile.ViewModels.User;
using Spendly.Shared.DataLayer;
using Spendly.Shared.Entities.Subscription;
using Spendly.Shared.Enums;
using Spendly.Shared.Localization;
using Spendly.Shared.ViewModels;
using Spendly.Shared.ViewModels.Settings;
using Stripe;
using Stripe.Checkout;
using System.Security.Claims;

public class UserSubscriptionService(
	IRepository<UserSubscription> userSubscriptionRepository,
	IRepository<UserSubscriptionPaymentUrl> userSubscriptionPaymentUrlRepository,
	LogRepository<StripeCommunicationLog> stripeCommunicationLogRepository,
	IRepository<FirebaseToken> firebaseTokenRepository,
	RequestContextViewModel requestContextViewModel,
	StripeSettings stripeSettings,
	JwtSettings jwtSettings,
	FirebaseNotificationService firebaseNotificationService,
	EmailService emailService,
	ILogger<UserSubscriptionService> logger)
{

	public async Task<FunctionResponse<List<SubscriptionPlanResponseViewModel>>> GetSubscriptionPlansAsync()
	{
		var plans = new List<SubscriptionPlanResponseViewModel>
		{
			new() {SubscriptionType = SubscriptionType.Free,DurationType = null, Price = 0},
			new() {SubscriptionType = SubscriptionType.Plus,DurationType = UserSubscriptionDurationType.Monthly, Price = SubscriptionService.Get(SubscriptionType.Plus).MonthlyPrice},
			new() {SubscriptionType = SubscriptionType.Plus,DurationType = UserSubscriptionDurationType.Yearly, Price = SubscriptionService.Get(SubscriptionType.Plus).YearlyPrice}
		};
		
		return FunctionResponse.Success(plans);
	}

	public async Task<FunctionResponse<CreatePaymentUrlResponseViewModel>> CreatePaymentUrlAsync(CreatePaymentUrlRequestViewModel request)
	{
		var userId = requestContextViewModel.UserId.ToObjectId();
		return await CreatePaymentUrlAsync(userId, request.DurationType, request.SubscriptionType);
	}

	public async Task<FunctionResponse<CreatePaymentUrlResponseViewModel>> CreatePaymentUrlWithTokenAsync(CreatePaymentUrlWithTokenRequestViewModel request)
	{
		// Validate access token and extract user ID
		var tokenValidation = ValidateAccessToken(request.AccessToken);
		if (!tokenValidation.isValid || tokenValidation.userId == null)
		{
			return FunctionResponse<CreatePaymentUrlResponseViewModel>.Failure(MessageCodes.InvalidToken);
		}

		return await CreatePaymentUrlAsync(tokenValidation.userId.Value, request.DurationType, request.SubscriptionType);
	}

	public async Task<FunctionResponse<CreatePaymentUrlResponseViewModel>> CreatePaymentUrlAsync(ObjectId userId, UserSubscriptionDurationType durationType, SubscriptionType subscriptionType)
	{
		var amount = durationType == UserSubscriptionDurationType.Monthly ? SubscriptionService.Get(subscriptionType).MonthlyPrice : SubscriptionService.Get(subscriptionType).YearlyPrice;

		var clientReferenceId = ObjectId.GenerateNewId().ToString();

		StripeConfiguration.ApiKey = stripeSettings.ApiKey;

		var durationName = durationType == UserSubscriptionDurationType.Monthly ? "Monthly" : "Yearly";

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
							Name = $"Spendly {durationName} {subscriptionType.ToString()} Subscription"
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
			await emailService.SendAlarmEmailAsync("Can not create payment url", ex);
			logger.LogError(ex,
				"Stripe payment session creation failed. UserId: {UserId}, PlanType: {PlanType}, ClientReferenceId: {ClientReferenceId}",
				userId,
				durationType,
				clientReferenceId);
			return FunctionResponse<CreatePaymentUrlResponseViewModel>.Failure("STRIPE_ERROR");
		}

		var responsePayload = System.Text.Json.JsonSerializer.Serialize(session);

		var paidUserSubscription = new UserSubscription
		{
			SubscriptionType = subscriptionType,
			UserId = userId,
			State = UserSubscriptionStateType.Waiting,
			Duration = durationType
		};
		await userSubscriptionRepository.InsertAsync(paidUserSubscription);

		await stripeCommunicationLogRepository.InsertAsync(new StripeCommunicationLog
		{
			ClientReferenceId = clientReferenceId,
			RequestPayload = requestPayload,
			ResponsePayload = responsePayload,
			Headers = string.Empty,
			UserId = requestContextViewModel.TryToGetUserId()
		});

		var paymentUrl = new UserSubscriptionPaymentUrl
		{
			Id = clientReferenceId.ToObjectId(),
			UserSubscriptionId = paidUserSubscription.Id,
			StripeSessionId = session.Id,
			PaymentUrl = session.Url,
			Payment = new UserSubscriptionPayment
			{
				Amount = amount,
				PaymentStatus = UserSubscriptionPaymentStatusType.Waiting
			},
		};

		await userSubscriptionPaymentUrlRepository.InsertAsync(paymentUrl);

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
			UserId = requestContextViewModel.TryToGetUserId()
		};

		await stripeCommunicationLogRepository.InsertAsync(log);

		var isPaymentComplete = false;
		try
		{
			var stripeEvent = EventUtility.ConstructEvent(
				payload,
				signature,
				stripeSettings.WebhookSecret
			);

			log.ResponsePayload = System.Text.Json.JsonSerializer.Serialize(stripeEvent.Data.Object);
			await stripeCommunicationLogRepository.UpdateAsync(log);

			//Failed webhooklarinin bir onemi yok, db de failed i islemenin de bir anlami yok.
			if (stripeEvent.Type != "checkout.session.completed")
			{
				logger.LogInformation($"Ignored the type: {stripeEvent.Type}");
				return FunctionResponse.Success();
			}

			//todo && stripeEvent.Type != "checkout.session.expired" bu durumda kullaniciyi bilgilendirmek faydali olabilir akisi yeniden baslatsin

			string? clientReferenceId = null;
			string? paymentIntentId = null;

			//tODO stripeEvent.id kullanilarak idempotency olusturulabilir

			if (stripeEvent.Data.Object is PaymentIntent paymentIntent)
			{
				paymentIntentId = paymentIntent.Id;
				if (paymentIntent.Metadata != null &&
				    paymentIntent.Metadata.TryGetValue("client_reference_id", out var metadataReferenceId))
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

			if (string.IsNullOrWhiteSpace(clientReferenceId) &&
			    !string.IsNullOrWhiteSpace(paymentIntentId))
			{
				var stripeSessionService = new SessionService(new StripeClient(stripeSettings.ApiKey));
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

			isPaymentComplete = true;

			log.ClientReferenceId = clientReferenceId;
			await stripeCommunicationLogRepository.UpdateAsync(log);

			var userSubscriptionPaymentUrl = await userSubscriptionPaymentUrlRepository.GetAsync(x => x.Id == clientReferenceId.ToObjectId());
			if (userSubscriptionPaymentUrl == null)
			{
				logger.LogWarning("Payment URL not found for ClientReferenceId: {ClientReferenceId}", clientReferenceId);
				return FunctionResponse.Failure("WEBHOOK_PAYMENT_URL_NOT_FOUND");
			}

			if (userSubscriptionPaymentUrl.Payment.PaymentStatus == UserSubscriptionPaymentStatusType.Paid)
			{
				return FunctionResponse.Success();
			}

			userSubscriptionPaymentUrl.Payment.PaymentCompletionDateTime = DateTime.UtcNow;
			userSubscriptionPaymentUrl.Payment.PaymentStatus = UserSubscriptionPaymentStatusType.Paid;
			await userSubscriptionPaymentUrlRepository.UpdateAsync(userSubscriptionPaymentUrl);

			var userSubscription = await userSubscriptionRepository.GetRequiredAsync(userSubscriptionPaymentUrl.UserSubscriptionId);

			var firebaseToken = await firebaseTokenRepository.GetRequiredAsync(p => p.UserId == userSubscription.UserId);

			userSubscription.StartDateTime = DateTime.UtcNow;
			userSubscription.ExpectedEndDateTime = userSubscription.Duration == UserSubscriptionDurationType.Monthly ? DateTime.UtcNow.AddMonths(1) : DateTime.UtcNow.AddYears(1);
			userSubscription.State = UserSubscriptionStateType.Active;

			await userSubscriptionRepository.UpdateAsync(userSubscription);

			logger.LogInformation("Subscription updated successfully. SubscriptionId: {SubscriptionId}, ClientReferenceId: {ClientReferenceId}",
				userSubscription.Id,
				clientReferenceId);

			await firebaseNotificationService.SendSubscriptionPaymentResultAsync(firebaseToken.Token, SubscriptionPaymentResultStatusType.Success);

			return FunctionResponse.Success();
		}
		catch (StripeException ex)
		{
			await emailService.SendAlarmEmailAsync("Stripe exception when handling webhook", ex);
			logger.LogError(ex, "Stripe webhook signature verification failed");
			return FunctionResponse.Failure("WEBHOOK_VERIFICATION_FAILED");
		}
		catch (Exception ex)
		{
			await emailService.SendAlarmEmailAsync($"Exception when handling webhook. isPaymentComplete: {isPaymentComplete}", ex);
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

	private (bool isValid, ObjectId? userId) ValidateAccessToken(string accessToken)
	{
		try
		{
			var tokenHandler = new JwtSecurityTokenHandler();
			var key = Encoding.UTF8.GetBytes(jwtSettings.SecretKey);

			var validationParameters = new TokenValidationParameters
			{
				ValidateIssuerSigningKey = true,
				IssuerSigningKey = new SymmetricSecurityKey(key),
				ValidateIssuer = true,
				ValidIssuer = jwtSettings.Issuer,
				ValidateAudience = true,
				ValidAudience = jwtSettings.Audience,
				ValidateLifetime = true,
				ClockSkew = TimeSpan.Zero
			};

			var principal = tokenHandler.ValidateToken(accessToken, validationParameters, out var validatedToken);
			
			if (validatedToken is not JwtSecurityToken jwtToken ||
			    !jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
			{
				return (false, null);
			}

			var userIdClaim = principal.FindFirst(ClaimTypes.Name)?.Value;
			if (string.IsNullOrEmpty(userIdClaim) || !ObjectId.TryParse(userIdClaim, out var userId))
			{
				return (false, null);
			}

			return (true, userId);
		}
		catch (Exception ex)
		{
			logger.LogWarning(ex, "Access token validation failed");
			return (false, null);
		}
	}
}
