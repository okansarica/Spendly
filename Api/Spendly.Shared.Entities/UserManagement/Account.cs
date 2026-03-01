namespace Spendly.Shared.Entities.UserManagement;

using Core;
using MongoDB.Bson;

public class Account : BaseEntity
{
    public ObjectId CurrencyId { get; set; }
    public ObjectId UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    
    public AccountType Type { get; set; }
}

public enum AccountType
{
    Bank = 0,
    CreditCard = 1,
    Cash = 2
}

