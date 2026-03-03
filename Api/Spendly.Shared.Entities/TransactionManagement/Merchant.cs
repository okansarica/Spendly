// CHANGED_BY_AI: 2026-03-02 - Add merchant nickname
// CHANGED_BY_AI: 2026-03-02 - Add merchant ownership and uncategorized support

namespace Spendly.Shared.Entities.TransactionManagement;

using Core;
using MongoDB.Bson;

public class Merchant : BaseEntity
{
    public ObjectId UserId { get; set; }
    public ObjectId? CategoryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Nickname { get; set; }
    public int TransactionCount { get; set; }
    public decimal TransactionAmount { get; set; }
}
