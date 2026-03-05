namespace Spendly.Shared.ViewModels.Settings;

public class JwtSettings 
{
	public required string SecretKey { get; set; }
	public required string Issuer { get; set; }
	public required string Audience { get; set; }
	public int RefreshRenewDays { get; set; } = 5;
}
