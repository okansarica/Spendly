namespace Spendly.Shared.Entities.UserManagement;

using Core;
using MongoDB.Bson;

public class FirebaseToken:BaseEntity
{
	public ObjectId? UserId { get; set; }
	public string Token { get; set; } = string.Empty;
	
}
