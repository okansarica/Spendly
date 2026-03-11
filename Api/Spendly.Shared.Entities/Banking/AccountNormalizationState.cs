namespace Spendly.Shared.Entities.Banking;

using Core;
using MongoDB.Bson;

public class AccountNormalizationState:BaseEntity
{
	public ObjectId AccountId { get; set; }
	public DateOnly Date { get; set; }
}
