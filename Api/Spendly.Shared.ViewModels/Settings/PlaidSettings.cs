namespace Spendly.Shared.ViewModels.Settings;

public class PlaidSettings
{
    public string ClientId { get; set; } = string.Empty;
    public string Secret { get; set; } = string.Empty;

    public string BaseUrl  { get; set; } = string.Empty;
    public string RedirectUrl { get; set; }  = string.Empty;
}
