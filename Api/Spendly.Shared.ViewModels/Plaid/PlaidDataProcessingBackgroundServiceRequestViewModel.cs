namespace Spendly.Shared.ViewModels.Plaid;

public class PlaidDataProcessingBackgroundServiceRequestViewModel
{
	public required string UserId { get; set; }
	public required string BankId { get; set; }
    
	//TODO access token aktarilmasin, db den cekilsin
	public required string AccessToken { get; set; } = string.Empty;
	//public List<string> NewAccountIds { get; set; } = [];

}
