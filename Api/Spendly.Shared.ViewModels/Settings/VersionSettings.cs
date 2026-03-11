namespace Spendly.Shared.ViewModels.Settings;

public class VersionSettings
{
	public string MinSupportedVersion { get; set; } = string.Empty;
	public bool ForceUpdate { get; set; }
	public string AppleStoreUrl { get; set; } = string.Empty;
	public string PlayStoreUrl { get; set; } = string.Empty;
	public List<VersionLocalizedMessage> LocalizedMessages { get; set; } = [];
}

public class VersionLocalizedMessage
{
	public string LanguageCode { get; set; } = string.Empty;
	public string Message { get; set; } = string.Empty;
}
