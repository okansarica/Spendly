namespace Spendly.Shared.Entities.UserManagement;

using Core;
using MongoDB.Bson;

public class Account : BaseEntity,ISoftDeletable
{
    public ObjectId BankId { get; set; }
    public string CurrencyCode { get; set; } = String.Empty;
    
    public string Name { get; set; } = string.Empty;
    public string? NickName { get; set; }
    public string? Mask { get; set; }
    public string? Description { get; set; }
   
    /// <summary>
    /// Shows if connected to open banking and the data is being queries autimatically
    /// </summary>
    public bool IsConnected { get; set; }
    
    public string? PlaidAccountId { get; set; }
    
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    
    public DateTime? ConnectionDateTime { get; set; }
}


