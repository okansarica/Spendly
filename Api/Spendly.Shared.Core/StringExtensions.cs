namespace Spendly.Shared.Core;

using MongoDB.Bson;

public static class StringExtensions
{
	public static ObjectId? ToObjectIdOrNull(this string? value)
	{
		if (string.IsNullOrWhiteSpace(value))
			return null;

		return ObjectId.TryParse(value, out var objectId) ? objectId : null;
	}
	
	public static ObjectId ToObjectId(this string value)
	{
		return ObjectId.Parse(value);
	}
}
