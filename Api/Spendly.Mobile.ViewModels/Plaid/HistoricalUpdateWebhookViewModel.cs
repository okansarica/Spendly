namespace Spendly.Mobile.ViewModels.Plaid;

using System.Text.Json.Serialization;

public class HistoricalUpdateWebhookViewModel
{
    [JsonPropertyName("webhook_type")]
    public string WebhookType { get; set; } = null!;

    [JsonPropertyName("webhook_code")]
    public string WebhookCode { get; set; } = null!;

    [JsonPropertyName("item_id")]
    public string ItemId { get; set; } = null!;

    [JsonPropertyName("new_transactions")]
    public int NewTransactions { get; set; }

    [JsonPropertyName("historical_update_complete")]
    public bool HistoricalUpdateComplete { get; set; }

    [JsonPropertyName("error")]
    public HistoricalUpdateError? Error { get; set; }
}

public class HistoricalUpdateError
{
    [JsonPropertyName("error_type")]
    public string ErrorType { get; set; } = null!;

    [JsonPropertyName("error_code")]
    public string ErrorCode { get; set; } = null!;

    [JsonPropertyName("error_message")]
    public string ErrorMessage { get; set; } = null!;
}
