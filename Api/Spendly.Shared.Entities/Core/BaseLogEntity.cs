namespace Spendly.Shared.Entities.Core;

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

public class BaseLogEntity
{
	[BsonId]
	public ObjectId Id { get; set; }
	
	[BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
	public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
