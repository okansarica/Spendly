using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace Spendly.Jobs.DataProcessor
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            var plaidSection = config.GetSection("Plaid");
            var clientId = plaidSection["ClientId"] ?? Environment.GetEnvironmentVariable("PLAID_CLIENT_ID");
            var secret = plaidSection["Secret"] ?? Environment.GetEnvironmentVariable("PLAID_SECRET");
            var environment = plaidSection["Environment"] ?? Environment.GetEnvironmentVariable("PLAID_ENVIRONMENT") ?? "sandbox";
            var sandboxPublicToken = plaidSection["SandboxPublicToken"] ?? Environment.GetEnvironmentVariable("PLAID_SANDBOX_PUBLIC_TOKEN");
            var accessTokenFromConfig = plaidSection["AccessToken"] ?? Environment.GetEnvironmentVariable("PLAID_ACCESS_TOKEN");

            Environment.SetEnvironmentVariable("PLAID_CLIENT_ID", clientId);
            Environment.SetEnvironmentVariable("PLAID_SECRET", secret);
            Environment.SetEnvironmentVariable("PLAID_ENVIRONMENT", environment);

            var httpClient = new HttpClient();
            var client = new PlaidClient(httpClient);

            var publicToken = sandboxPublicToken ?? "";
            var exchange = string.IsNullOrEmpty(publicToken) ? null : await client.ExchangePublicTokenAsync(publicToken);
            var accessToken = exchange?.AccessToken ?? accessTokenFromConfig ?? string.Empty;

            var startDate = DateTime.UtcNow.AddDays(-30);
            var endDate = DateTime.UtcNow;

            var transactionsResp = await client.GetTransactionsAsync(accessToken, startDate, endDate, count: 50, offset: 0);

            Console.WriteLine($"Total transactions: {transactionsResp?.TotalTransactions}");
            if (transactionsResp?.Transactions != null)
            {
                foreach (var t in transactionsResp.Transactions)
                {
                    Console.WriteLine($"{t.Date} | {t.Name} | {t.Amount} {t.IsoCurrencyCode}");
                }
            }
        }
    }
}
