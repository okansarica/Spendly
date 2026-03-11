// CHANGED_BY_AI: 2026-03-10 - Add new account plaid id filter for scoped background processing
namespace Spendly.Shared.ViewModels.Plaid;

public class PlaidDataProcessingBackgroundServiceRequestViewModel
{
	public required string UserId { get; set; }
	public required string BankId { get; set; }
    
	//TODO access token aktarilmasin, db den cekilsin
	public required string AccessToken { get; set; } = string.Empty;
	
	/// <summary>
	/// Kullanici plaid update mode a girip hesaplari tekrar sectiginde, her zaman son secili mesajlar gecerlidir. Bu liste en son secilen account listesini tutar
	/// </summary>
	public List<string> NewAccountPlaidIds { get; set; } = [];
}
