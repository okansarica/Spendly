namespace Spendly.Shared.Entities.TransactionManagement;

using Core;
using MongoDB.Bson;

public class RawTransaction : BaseEntity
{
    public ObjectId UserId { get; set; }
    public DateTime DateTime { get; set; }
    public decimal Amount { get; set; }
    public string MerchantName { get; set; } = string.Empty;
    public string RawMerchant { get; set; } =  string.Empty;
}

