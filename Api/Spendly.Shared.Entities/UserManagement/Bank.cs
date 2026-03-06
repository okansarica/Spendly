namespace Spendly.Shared.Entities.UserManagement;

using Core;
using MongoDB.Bson;

public class Bank:BaseEntity
{
	public ObjectId UserId { get; set; }
	public string Name { get; set; } =  string.Empty;
	public string AccessToken { get; set; } =  string.Empty;
}
