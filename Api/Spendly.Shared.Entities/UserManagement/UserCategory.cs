// CHANGED_BY_AI: 2026-03-02 - Add category display fields
namespace Spendly.Shared.Entities.UserManagement;

using Core;
using MongoDB.Bson;

/// <summary>
/// Predefined categories must be copied to user
/// </summary>
public class Category : BaseEntity
{
    public ObjectId? ParentId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Color { get; set; }
    public string? Icon { get; set; }
    
    /// <summary>
    /// We need a defined Other category to make reports more sense, otherwise we dont show the merchant and connected transaction because they dont have a category. This category can not be updated/deleted
    /// </summary>
    public bool IsOther { get; set; }
}


public class UserCategory : BaseEntity
{
    public ObjectId UserId { get; set; }
    public ObjectId? ParentId { get; set; }
    
    /// <summary>
    /// Kullanicilar ilk uye olduklarinda predefined categoriler kullaniciya kopyalanir, sonrasinda kullanici istedigi gibi degistirebilir. Eger kullanici degistirmemisse CategoryId si dolu kalir, bu da bize user merchantlar olustururken hangi kategoriye bagli olduklarini bulmakta kolaylik yaratir
    /// </summary>
    public ObjectId? CategoryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Color { get; set; }
    public string? Icon { get; set; }
    public int MerchantCount { get; set; }
    
    /// <summary>
    /// We need a defined Other category to make reports more sense, otherwise we dont show the merchant and connected transaction because they dont have a category. This category can not be updated/deleted
    /// </summary>
    public bool IsOther { get; set; }
}
