namespace Spendly.Shared.Entities.TransactionManagement;

using Core;
using MongoDB.Bson;

public class RawTransaction : BaseEntity
{
    public ObjectId UserId { get; set; }
    public ObjectId AccountId { get; set; }
    public DateTime DateTime { get; set; }
    public string? Description { get; set; }
    public decimal Amount { get; set; }
    public string RawCategory { get; set; }  = string.Empty;
    public string RawMerchant { get; set; }   = string.Empty;
}

