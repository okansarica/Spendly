namespace Spendly.Shared.ViewModels.Settings;

public class StripeSettings
{
	public string SuccessUrl { get; set; } = string.Empty;
	public string CancelUrl { get; set; } = string.Empty;
	public string WebhookSecret { get; set; } = string.Empty;
	public string ApiKey { get; set; } = string.Empty;
}

