namespace Spendly.Shared.Entities.Banking;

using Core;

public class PlaidCommunicationLog:BaseEntity
{
	public string Content { get; set; }=string.Empty;
	public PlaidCommunicationLogType Type { get; set; }
	
}

public enum PlaidCommunicationLogType
{
	CreateTLinkTokenRequest,
	CreateTLinkTokenResponse,
	ExchangePublicTokenRequest,
	ExchangePublicTokenResponse,
	GetItemRequest,
	GetItemResponse,
	GetInstitutionRequest,
	GetInstitutionResponse,
	GetAccountsRequest,
	GetAccountsResponse,
}
