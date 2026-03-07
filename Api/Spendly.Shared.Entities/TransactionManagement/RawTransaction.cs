namespace Spendly.Shared.Entities.TransactionManagement;

using Core;
using MongoDB.Bson;

public class RawTransaction : BaseEntity
{
    public ObjectId UserId { get; set; }
    public DateOnly Date { get; set; }
    public bool Initial { get; set; }

    public PlaidTransactionsGetResponse PlaidTransactionsGetResponse { get; set; } = new();

}
public class PlaidTransactionsGetResponse
{
    public List<PlaidTransactionAccount>? Accounts { get; set; }

    public IEnumerable<PlaidTransaction> Transactions { get; set; } = [];

    public int TotalTransactions { get; set; }

    public PlaidTransactionItem? Item { get; set; }

    public string? RequestId { get; set; }
}

public class PlaidTransaction
{
    public string TransactionId { get; set; } = string.Empty;

    public string AccountId { get; set; } = string.Empty;

    public decimal Amount { get; set; }

    public DateOnly Date { get; set; }

    public DateOnly? AuthorizedDate { get; set; }

    public DateTime? AuthorizedDateTime { get; set; }

    public DateTime? DateTime { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? MerchantName { get; set; }

    public bool Pending { get; set; }

    public string? PendingTransactionId { get; set; }

    public string? AccountOwner { get; set; }

    public List<string>? Category { get; set; }

    public string? CategoryId { get; set; }

    public string? CheckNumber { get; set; }

    public string? TransactionCode { get; set; }

    public string? TransactionType { get; set; }

    public string? PaymentChannel { get; set; }

    public string? IsoCurrencyCode { get; set; }

    public string? UnofficialCurrencyCode { get; set; }

    public string? MerchantEntityId { get; set; }

    public string? LogoUrl { get; set; }

    public string? Website { get; set; }

    public string? PersonalFinanceCategoryIconUrl { get; set; }

    public List<PlaidTransactionCounterparty>? Counterparties { get; set; }

    public PlaidTransactionLocation? Location { get; set; }

    public PlaidTransactionPaymentMeta? PaymentMeta { get; set; }

    public PlaidTransactionPersonalFinanceCategory? PersonalFinanceCategory { get; set; }
}

public class PlaidTransactionAccount
{
    public string? AccountId { get; set; }

    public PlaidTransactionAccountBalances? Balances { get; set; }

    public string? Mask { get; set; }

    public string? Name { get; set; }

    public string? OfficialName { get; set; }

    public string? Type { get; set; }

    public string? Subtype { get; set; }
}

public class PlaidTransactionAccountBalances
{
    public decimal? Available { get; set; }

    public decimal? Current { get; set; }

    public decimal? Limit { get; set; }

    public string? IsoCurrencyCode { get; set; }

    public string? UnofficialCurrencyCode { get; set; }
}

public class PlaidTransactionItem
{
    public string? ItemId { get; set; }

    public string? InstitutionId { get; set; }

    public string? Webhook { get; set; }

    public PlaidTransactionError? Error { get; set; }

    public List<string>? AvailableProducts { get; set; }

    public List<string>? BilledProducts { get; set; }

    public List<string>? Products { get; set; }

    public DateTime? ConsentExpirationTime { get; set; }

    public string? UpdateType { get; set; }
}

public class PlaidTransactionError
{
    public string? ErrorType { get; set; }

    public string? ErrorCode { get; set; }

    public string? ErrorMessage { get; set; }

    public string? DisplayMessage { get; set; }

    public string? RequestId { get; set; }

    public List<PlaidTransactionErrorCause>? Causes { get; set; }

    public int? Status { get; set; }

    public string? DocumentationUrl { get; set; }

    public string? SuggestedAction { get; set; }
}

public class PlaidTransactionErrorCause
{
    public string? ItemId { get; set; }

    public string? ErrorType { get; set; }

    public string? ErrorCode { get; set; }

    public string? ErrorMessage { get; set; }

    public string? DisplayMessage { get; set; }
}

public class PlaidTransactionCounterparty
{
    public string? Name { get; set; }

    public string? EntityId { get; set; }

    public string? Type { get; set; }

    public string? Website { get; set; }

    public string? LogoUrl { get; set; }

    public string? ConfidenceLevel { get; set; }
}

public class PlaidTransactionLocation
{
    public string? Address { get; set; }

    public string? City { get; set; }

    public string? Region { get; set; }

    public string? PostalCode { get; set; }

    public string? Country { get; set; }

    public decimal? Lat { get; set; }

    public decimal? Lon { get; set; }

    public string? StoreNumber { get; set; }
}

public class PlaidTransactionPaymentMeta
{
    public string? ByOrderOf { get; set; }

    public string? Payee { get; set; }

    public string? Payer { get; set; }

    public string? PaymentMethod { get; set; }

    public string? PaymentProcessor { get; set; }

    public string? PpdId { get; set; }

    public string? Reason { get; set; }

    public string? ReferenceNumber { get; set; }

    public string? Receiver { get; set; }
}

public class PlaidTransactionPersonalFinanceCategory
{
    public string? Primary { get; set; }

    public string? Detailed { get; set; }

    public string? ConfidenceLevel { get; set; }
}
