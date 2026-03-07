namespace Spendly.Shared.Entities.UserManagement;

using Core;
using MongoDB.Bson;

public class Bank:BaseEntity,ISoftDeletable
{
	public ObjectId UserId { get; set; }
	public ObjectId? BankDefinitionId { get; set; }
	public ObjectId? UserPlaidTokenId { get; set; }
	
	/// <summary>
	/// When the BankDefinitionId is populated this will be empty
	/// </summary>
	public string? Name { get; set; } =  string.Empty;
	public string? Description { get; set; }

	/// <summary>
	/// Shows if connected to open banking and the data is being queries autimatically
	/// </summary>
	public bool IsConnected { get; set; }
	
	//public string? AccessToken { get; set; }
	
	public string? PlaidInstitutionId { get; set; }
	public string? PlaidItemId { get; set; }
	
	public bool IsDeleted { get; set; }
	public DateTime? DeletedAt { get; set; }

	public DateTime? ConnectionDateTime { get; set; }
}

public class BankDefinition : BaseEntity
{
	public string Name { get; set; } = string.Empty;
	public string? LogoName { get; set; }
}
