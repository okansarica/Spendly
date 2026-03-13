namespace Spendly.Shared.Entities.Core;

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

public class BaseEntity
{
	[BsonId]
	public ObjectId Id { get; set; }
	
	[BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
	public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

	[BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
	public DateTime? UpdatedAt { get; set; }
}

public class BaseReportEntity : BaseEntity
{
	public ObjectId UserId { get; set; }
}

//Local mongodb queue icin kullaniliyor
public class BaseLocalQueueEntity : BaseEntity
{
	public ObjectId UserId { get; set; }
}

public interface ISoftDeletable
{
	bool IsDeleted { get; set; }
	
	DateTime? DeletedAt { get; set; }
}
