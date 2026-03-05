// CHANGED_BY_AI: 2026-03-05 - Add reusable Firebase push notification service
namespace Spendly.Mobile.BusinessLayer.Services.Notification;

using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Spendly.Shared.Enums;
using Spendly.Shared.ViewModels.Settings;

public class FirebaseNotificationService(FirebaseSettings firebaseSettings, ILogger<FirebaseNotificationService> logger)
{
	public async  virtual Task SendSubscriptionPaymentResultAsync(string token, SubscriptionPaymentResultStatusType status)
	{
		try
		{
			using var httpClient = new HttpClient();
			httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", $"key={firebaseSettings.ServerKey}");

			var payload = new
			{
				to = token,
				data = new
				{
					type = "subscription_payment_result",
					status = status.ToString().ToLowerInvariant(),
				}
			};

			var requestBody = JsonSerializer.Serialize(payload);
			using var response = await httpClient.PostAsync(
				firebaseSettings.Endpoint,
				new StringContent(requestBody, Encoding.UTF8, "application/json"));

			if (!response.IsSuccessStatusCode)
			{
				var responseBody = await response.Content.ReadAsStringAsync();
				logger.LogWarning("Firebase notification send failed. StatusCode: {StatusCode}, Response: {Response}", response.StatusCode, responseBody);
			}
			
			//tODO responseda gonderilen sayiyi kontrol et, gonderinin basarili oldugunu verify et
		}
		catch (Exception e)
		{
			//TODO hataya duserse alarm e postasi gondermek gerekiyor
		}
		
	}
}
