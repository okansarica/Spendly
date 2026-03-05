// CHANGED_BY_AI: 2026-03-05 - Add Firebase settings for push notifications
namespace Spendly.Shared.ViewModels.Settings;

public class FirebaseSettings
{
	public string Endpoint { get; set; } = "https://fcm.googleapis.com/fcm/send";
	public string ServerKey { get; set; } = string.Empty;
}

