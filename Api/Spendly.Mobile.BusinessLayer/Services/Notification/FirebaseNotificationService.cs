// CHANGED_BY_AI: 2026-03-05 - Add reusable Firebase push notification service
namespace Spendly.Mobile.BusinessLayer.Services.Notification;

using FirebaseAdmin.Messaging;
using Microsoft.Extensions.Logging;
using Spendly.Shared.Enums;

public class FirebaseNotificationService(ILogger<FirebaseNotificationService> logger)
{
	public async virtual Task SendSubscriptionPaymentResultAsync(string token, SubscriptionPaymentResultStatusType status)
	{
		try
		{
			var message = new Message
			{
				Token = token,
				Data = new Dictionary<string, string>
				{
					{ "type", "subscription_payment_result" },
					{ "status", status.ToString().ToLowerInvariant() }
				}
			};

			await FirebaseMessaging.DefaultInstance.SendAsync(message);
			
			//tODO responseda gonderilen sayiyi kontrol et, gonderinin basarili oldugunu verify et
		}
		catch (Exception)
		{
			//TODO hataya duserse alarm e postasi gondermek gerekiyor
		}

	}
}
