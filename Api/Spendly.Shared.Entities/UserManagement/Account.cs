namespace Spendly.Shared.Entities.UserManagement;

using Core;
using MongoDB.Bson;

public class Account : BaseEntity
{
    public ObjectId BankId { get; set; }
    public ObjectId CurrencyId { get; set; }
    
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    
    public AccountType Type { get; set; }
    
    /// <summary>
    /// Shows if connected to open banking and the data is being queries autimatically
    /// </summary>
    public bool IsConnected { get; set; }
}

public enum AccountType
{
    Bank = 0,
    CreditCard = 1,
    Cash = 2
}

