namespace Spendly.Shared.Entities.UserManagement;

using Core;
using MongoDB.Bson;

public class Category : BaseEntity
{
    public ObjectId UserId { get; set; }
    public ObjectId? ParentId { get; set; }
    public string Name { get; set; } = string.Empty;
}

