namespace Spendly.Shared.ViewModels.Plaid;

using System.Text.Json.Serialization;

public class PlaidTransactionsGetResponseViewModel
{
    [JsonPropertyName("accounts")]
    public List<PlaidTransactionAccountViewModel>? Accounts { get; set; }

    [JsonPropertyName("transactions")]
    public List<PlaidTransactionViewModel> Transactions { get; set; } = [];

    [JsonPropertyName("total_transactions")]
    public int TotalTransactions { get; set; }

    [JsonPropertyName("item")]
    public PlaidTransactionItemViewModel? Item { get; set; }

    [JsonPropertyName("request_id")]
    public string? RequestId { get; set; }
}

public class PlaidTransactionViewModel
{
    [JsonPropertyName("transaction_id")]
    public string TransactionId { get; set; } = string.Empty;

    [JsonPropertyName("account_id")]
    public string AccountId { get; set; } = string.Empty;

    [JsonPropertyName("amount")]
    public decimal Amount { get; set; }

    [JsonPropertyName("date")]
    public DateOnly Date { get; set; }

    [JsonPropertyName("authorized_date")]
    public DateOnly? AuthorizedDate { get; set; }

    [JsonPropertyName("authorized_datetime")]
    public DateTime? AuthorizedDateTime { get; set; }

    [JsonPropertyName("datetime")]
    public DateTime? DateTime { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("merchant_name")]
    public string? MerchantName { get; set; }

    [JsonPropertyName("pending")]
    public bool Pending { get; set; }

    [JsonPropertyName("pending_transaction_id")]
    public string? PendingTransactionId { get; set; }

    [JsonPropertyName("account_owner")]
    public string? AccountOwner { get; set; }

    [JsonPropertyName("category")]
    public List<string>? Category { get; set; }

    [JsonPropertyName("category_id")]
    public string? CategoryId { get; set; }

    [JsonPropertyName("check_number")]
    public string? CheckNumber { get; set; }

    [JsonPropertyName("transaction_code")]
    public string? TransactionCode { get; set; }

    [JsonPropertyName("transaction_type")]
    public string? TransactionType { get; set; }

    [JsonPropertyName("payment_channel")]
    public string? PaymentChannel { get; set; }

    [JsonPropertyName("iso_currency_code")]
    public string? IsoCurrencyCode { get; set; }

    [JsonPropertyName("unofficial_currency_code")]
    public string? UnofficialCurrencyCode { get; set; }

    [JsonPropertyName("merchant_entity_id")]
    public string? MerchantEntityId { get; set; }

    [JsonPropertyName("logo_url")]
    public string? LogoUrl { get; set; }

    [JsonPropertyName("website")]
    public string? Website { get; set; }

    [JsonPropertyName("personal_finance_category_icon_url")]
    public string? PersonalFinanceCategoryIconUrl { get; set; }

    [JsonPropertyName("counterparties")]
    public List<PlaidTransactionCounterpartyViewModel>? Counterparties { get; set; }

    [JsonPropertyName("location")]
    public PlaidTransactionLocationViewModel? Location { get; set; }

    [JsonPropertyName("payment_meta")]
    public PlaidTransactionPaymentMetaViewModel? PaymentMeta { get; set; }

    [JsonPropertyName("personal_finance_category")]
    public PlaidTransactionPersonalFinanceCategoryViewModel? PersonalFinanceCategory { get; set; }
}

public class PlaidTransactionAccountViewModel
{
    [JsonPropertyName("account_id")]
    public string? AccountId { get; set; }

    [JsonPropertyName("balances")]
    public PlaidTransactionAccountBalancesViewModel? Balances { get; set; }

    [JsonPropertyName("mask")]
    public string? Mask { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("official_name")]
    public string? OfficialName { get; set; }

    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("subtype")]
    public string? Subtype { get; set; }
}

public class PlaidTransactionAccountBalancesViewModel
{
    [JsonPropertyName("available")]
    public decimal? Available { get; set; }

    [JsonPropertyName("current")]
    public decimal? Current { get; set; }

    [JsonPropertyName("limit")]
    public decimal? Limit { get; set; }

    [JsonPropertyName("iso_currency_code")]
    public string? IsoCurrencyCode { get; set; }

    [JsonPropertyName("unofficial_currency_code")]
    public string? UnofficialCurrencyCode { get; set; }
}

public class PlaidTransactionItemViewModel
{
    [JsonPropertyName("item_id")]
    public string? ItemId { get; set; }

    [JsonPropertyName("institution_id")]
    public string? InstitutionId { get; set; }

    [JsonPropertyName("webhook")]
    public string? Webhook { get; set; }

    [JsonPropertyName("error")]
    public PlaidTransactionErrorViewModel? Error { get; set; }

    [JsonPropertyName("available_products")]
    public List<string>? AvailableProducts { get; set; }

    [JsonPropertyName("billed_products")]
    public List<string>? BilledProducts { get; set; }

    [JsonPropertyName("products")]
    public List<string>? Products { get; set; }

    [JsonPropertyName("consent_expiration_time")]
    public DateTime? ConsentExpirationTime { get; set; }

    [JsonPropertyName("update_type")]
    public string? UpdateType { get; set; }
}

public class PlaidTransactionErrorViewModel
{
    [JsonPropertyName("error_type")]
    public string? ErrorType { get; set; }

    [JsonPropertyName("error_code")]
    public string? ErrorCode { get; set; }

    [JsonPropertyName("error_message")]
    public string? ErrorMessage { get; set; }

    [JsonPropertyName("display_message")]
    public string? DisplayMessage { get; set; }

    [JsonPropertyName("request_id")]
    public string? RequestId { get; set; }

    [JsonPropertyName("causes")]
    public List<PlaidTransactionErrorCauseViewModel>? Causes { get; set; }

    [JsonPropertyName("status")]
    public int? Status { get; set; }

    [JsonPropertyName("documentation_url")]
    public string? DocumentationUrl { get; set; }

    [JsonPropertyName("suggested_action")]
    public string? SuggestedAction { get; set; }
}

public class PlaidTransactionErrorCauseViewModel
{
    [JsonPropertyName("item_id")]
    public string? ItemId { get; set; }

    [JsonPropertyName("error_type")]
    public string? ErrorType { get; set; }

    [JsonPropertyName("error_code")]
    public string? ErrorCode { get; set; }

    [JsonPropertyName("error_message")]
    public string? ErrorMessage { get; set; }

    [JsonPropertyName("display_message")]
    public string? DisplayMessage { get; set; }
}

public class PlaidTransactionCounterpartyViewModel
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("entity_id")]
    public string? EntityId { get; set; }

    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("website")]
    public string? Website { get; set; }

    [JsonPropertyName("logo_url")]
    public string? LogoUrl { get; set; }

    [JsonPropertyName("confidence_level")]
    public string? ConfidenceLevel { get; set; }
}

public class PlaidTransactionLocationViewModel
{
    [JsonPropertyName("address")]
    public string? Address { get; set; }

    [JsonPropertyName("city")]
    public string? City { get; set; }

    [JsonPropertyName("region")]
    public string? Region { get; set; }

    [JsonPropertyName("postal_code")]
    public string? PostalCode { get; set; }

    [JsonPropertyName("country")]
    public string? Country { get; set; }

    [JsonPropertyName("lat")]
    public decimal? Lat { get; set; }

    [JsonPropertyName("lon")]
    public decimal? Lon { get; set; }

    [JsonPropertyName("store_number")]
    public string? StoreNumber { get; set; }
}

public class PlaidTransactionPaymentMetaViewModel
{
    [JsonPropertyName("by_order_of")]
    public string? ByOrderOf { get; set; }

    [JsonPropertyName("payee")]
    public string? Payee { get; set; }

    [JsonPropertyName("payer")]
    public string? Payer { get; set; }

    [JsonPropertyName("payment_method")]
    public string? PaymentMethod { get; set; }

    [JsonPropertyName("payment_processor")]
    public string? PaymentProcessor { get; set; }

    [JsonPropertyName("ppd_id")]
    public string? PpdId { get; set; }

    [JsonPropertyName("reason")]
    public string? Reason { get; set; }

    [JsonPropertyName("reference_number")]
    public string? ReferenceNumber { get; set; }

    [JsonPropertyName("receiver")]
    public string? Receiver { get; set; }
}

public class PlaidTransactionPersonalFinanceCategoryViewModel
{
    [JsonPropertyName("primary")]
    public string? Primary { get; set; }

    [JsonPropertyName("detailed")]
    public string? Detailed { get; set; }

    [JsonPropertyName("confidence_level")]
    public string? ConfidenceLevel { get; set; }
}
