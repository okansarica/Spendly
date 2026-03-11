namespace Spendly.Shared.Entities.Banking;

using Core;
using MongoDB.Bson;

/// <summary>
/// Her gun her account plaidden sorgulaniyor. bu tabloda hangi accountun en son ne zaman sorgulandigini tutar, her account icin 1 kayit olur, degistiginde ayni kayit guncellenir. Lookup tarzi
/// </summary>
public class AccountNormalizationState:BaseEntity
{
	public ObjectId AccountId { get; set; }
	public DateOnly Date { get; set; }
}
