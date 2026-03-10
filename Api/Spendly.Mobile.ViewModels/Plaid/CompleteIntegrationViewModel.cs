// CHANGED_BY_AI: 2026-03-10 - Add mode-aware Plaid request models for update account linking
namespace Spendly.Mobile.ViewModels.Plaid;

using MongoDB.Bson;

public enum PlaidFlowMode
{
	Create,
	Update,
}

public class CreateLinkTokenRequestViewModel
{
	public PlaidFlowMode Mode { get; set; } = PlaidFlowMode.Create;
	public string? BankId { get; set; }
}

public class CompleteIntegrationRequestViewModel
{
	public string PublicToken { get; set; } = null!;
	public PlaidFlowMode Mode { get; set; } = PlaidFlowMode.Create;
	public string? BankId { get; set; }
	public List<PlaidAccountViewModel> Accounts { get; set; } = new();
	public PlaidInstitutionViewModel Institution { get; set; } = null!;
	public string LinkSessionId { get; set; } = null!;
}

public class PlaidAccountViewModel
{
	public string? VerificationStatus { get; set; }

	public string Type { get; set; } = null!;

	public string? Mask { get; set; }

	public string Name { get; set; }

	public string Subtype { get; set; } = null!;

	public string Id { get; set; } = null!;
}

public class PlaidInstitutionViewModel
{
	public string Name { get; set; } = null!;

	public string Id { get; set; } = null!;
}

public class CompleteIntegrationResponseViewModel
{
	public required string AccessToken { get; set; }
	public required string BankId { get; set; }
	public required List<string> NewAccountPlaidIds { get; set; }
}
