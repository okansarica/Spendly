namespace Spendly.Jobs.DataProcessor
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Text;
    using System.Text.Json;
    using System.Text.Json.Serialization;
    using System.Threading.Tasks;

    public class PlaidClient
    {
        private readonly HttpClient _httpClient;
        private readonly string _clientId;
        private readonly string _secret;
        private readonly string _baseUrl;

        public PlaidClient(HttpClient httpClient)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _clientId = Environment.GetEnvironmentVariable("PLAID_CLIENT_ID") ?? string.Empty;
            _secret = Environment.GetEnvironmentVariable("PLAID_SECRET") ?? string.Empty;
            var env = Environment.GetEnvironmentVariable("PLAID_ENVIRONMENT") ?? "sandbox";
            _baseUrl = env switch
            {
                "sandbox" => "https://sandbox.plaid.com",
                "development" => "https://development.plaid.com",
                "production" => "https://production.plaid.com",
                _ => "https://sandbox.plaid.com",
            };
        }

        public async Task<ExchangeResponse?> ExchangePublicTokenAsync(string publicToken)
        {
            var url = new Uri(new Uri(_baseUrl), "/item/public_token/exchange");
            var request = new Dictionary<string, object>
            {
                ["client_id"] = _clientId,
                ["secret"] = _secret,
                ["public_token"] = publicToken
            };

            var json = JsonSerializer.Serialize(request);
            using var content = new StringContent(json, Encoding.UTF8, "application/json");
            using var resp = await _httpClient.PostAsync(url, content).ConfigureAwait(false);
            resp.EnsureSuccessStatusCode();
            var stream = await resp.Content.ReadAsStreamAsync().ConfigureAwait(false);
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            return await JsonSerializer.DeserializeAsync<ExchangeResponse>(stream, options).ConfigureAwait(false);
        }

        public async Task<TransactionsResponse?> GetTransactionsAsync(string accessToken, DateTime startDate, DateTime endDate, int? count = null, int? offset = null)
        {
            var url = new Uri(new Uri(_baseUrl), "/transactions/get");

            var request = new Dictionary<string, object>
            {
                ["client_id"] = _clientId,
                ["secret"] = _secret,
                ["access_token"] = accessToken,
                ["start_date"] = startDate.ToString("yyyy-MM-dd"),
                ["end_date"] = endDate.ToString("yyyy-MM-dd")
            };

            if (count.HasValue || offset.HasValue)
            {
                var options = new Dictionary<string, int>();
                if (count.HasValue) options["count"] = count.Value;
                if (offset.HasValue) options["offset"] = offset.Value;
                request["options"] = options;
            }

            var json = JsonSerializer.Serialize(request);
            using var content = new StringContent(json, Encoding.UTF8, "application/json");
            using var resp = await _httpClient.PostAsync(url, content).ConfigureAwait(false);
            resp.EnsureSuccessStatusCode();

            var respStream = await resp.Content.ReadAsStreamAsync().ConfigureAwait(false);
            var serializerOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var result = await JsonSerializer.DeserializeAsync<TransactionsResponse>(respStream, serializerOptions).ConfigureAwait(false);
            return result;
        }
    }

    public class ExchangeResponse
    {
        [JsonPropertyName("access_token")]
        public string? AccessToken { get; set; }

        [JsonPropertyName("item_id")]
        public string? ItemId { get; set; }

        [JsonPropertyName("request_id")]
        public string? RequestId { get; set; }
    }

    public class TransactionsResponse
    {
        [JsonPropertyName("transactions")]
        public Transaction[] Transactions { get; set; } = Array.Empty<Transaction>();

        [JsonPropertyName("total_transactions")]
        public int TotalTransactions { get; set; }

        [JsonPropertyName("request_id")]
        public string? RequestId { get; set; }
    }

    public class Transaction
    {
        [JsonPropertyName("transaction_id")]
        public string? Id { get; set; }

        [JsonPropertyName("account_id")]
        public string? AccountId { get; set; }

        [JsonPropertyName("amount")]
        public decimal Amount { get; set; }

        [JsonPropertyName("iso_currency_code")]
        public string? IsoCurrencyCode { get; set; }

        [JsonPropertyName("date")]
        public string? Date { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("merchant_name")]
        public string? MerchantName { get; set; }

        [JsonPropertyName("category")]
        public string[]? Category { get; set; }

        [JsonPropertyName("pending")]
        public bool Pending { get; set; }

        [JsonPropertyName("pending_transaction_id")]
        public string? PendingTransactionId { get; set; }
    }
}

