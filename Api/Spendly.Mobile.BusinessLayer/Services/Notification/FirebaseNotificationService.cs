// CHANGED_BY_AI: 2026-03-05 - Add reusable Firebase push notification service
namespace Spendly.Mobile.BusinessLayer.Services.Notification;

using FirebaseAdmin.Messaging;
using Microsoft.Extensions.Logging;
using Shared.BusinessLayer;
using Spendly.Shared.Enums;

public class FirebaseNotificationService(ILogger<FirebaseNotificationService> logger, EmailService emailService)
{
	public async virtual Task<bool> SendSubscriptionPaymentResultAsync(string token, SubscriptionPaymentResultStatusType status)
	{
		try
		{
			var message = new Message
			{
				Token = token,
				Data = new Dictionary<string, string>
				{
					{ "type", "subscription_payment_result" },
					{ "status", status.ToString().ToLowerInvariant() },
				}
			};

			var messageId = await FirebaseMessaging.DefaultInstance.SendAsync(message);

			logger.LogInformation("Firebase message sent. MessageId: {MessageId}", messageId);

			return true;
		}
		catch (Exception exception)
		{
			logger.LogError(exception, exception.Message);
			await emailService.SendAlarmEmailAsync("Error when sending firebase notification", exception);
			return false;
		}
	}
}
