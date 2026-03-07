namespace Spendly.Shared.Entities.Plaid;

using MongoDB.Bson;
using Spendly.Shared.Entities.Core;

public class UserDataProcessState:BaseEntity
{
	public ObjectId User { get; set; }
	public DateOnly Date { get; set; }
	public bool IsDataGatheredFromPlaid { get; set; }
	public bool IsDataNormalized { get; set; }
	
}
