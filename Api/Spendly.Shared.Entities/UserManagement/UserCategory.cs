// CHANGED_BY_AI: 2026-03-02 - Add category display fields
namespace Spendly.Shared.Entities.UserManagement;

using Core;
using MongoDB.Bson;

/// <summary>
/// Predefined categories must be copied to user
/// </summary>
public class Category : BaseEntity
{
    public ObjectId UserId { get; set; }
    public ObjectId? ParentId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Color { get; set; }
    public string? Icon { get; set; }
}


public class UserCategory : BaseEntity
{
    public ObjectId UserId { get; set; }
    public ObjectId? ParentId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Color { get; set; }
    public string? Icon { get; set; }
    public int MerchantCount { get; set; }
}
