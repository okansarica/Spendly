namespace Spendly.Mobile.Api.Services;

using Shared.BusinessLayer;
using Shared.Core;
using System.Threading.Channels;
using Spendly.Mobile.BusinessLayer.Services.Finance;

public class PlaidDataProcessorHostedService(
    Channel<PlaidDataProcessingRequest> channel,
    IServiceScopeFactory serviceScopeFactory,
    ILogger<PlaidDataProcessorHostedService> logger)
    : BackgroundService
{

    protected async override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var request in channel.Reader.ReadAllAsync(stoppingToken))
        {
            try
            {
                logger.LogInformation("Starting Plaid data processing for user {UserId}", request.UserId);
                
                using var scope = serviceScopeFactory.CreateScope();

                var sharedPlaidSservice = scope.ServiceProvider.GetRequiredService<SharedPlaidService>();
                await sharedPlaidSservice.QueryAndSaveUserTransactionAsync(
                    DateOnly.FromDateTime(DateTime.Today.AddDays(-1)),
                    DateOnly.FromDateTime(DateTime.Today.AddDays(-91)), // son gun dahil degil, bitis tarihi dahil, bugunun kayitlari gece cekilecek onlari cekme
                    request.UserId.ToObjectId()
                );
                
                // var plaidDataProcessingService = scope.ServiceProvider.GetRequiredService<PlaidDataProcessingService>();
                // var plaidService = scope.ServiceProvider.GetRequiredService<PlaidService>();
                // var transactionNormalizationService = scope.ServiceProvider.GetRequiredService<TransactionNormalizationService>();
                //
                // var savedRawTransactionIds = await plaidDataProcessingService.ProcessUserTransactionsAsync(
                //     request.UserId, 
                //     plaidService.GetTransactionsAsync);
                
                // logger.LogInformation("Starting normalization for user {UserId} with {Count} raw transactions", 
                //     request.UserId, savedRawTransactionIds.Count);
                //
                // await transactionNormalizationService.NormalizeTransactionsAsync(savedRawTransactionIds);
                
                logger.LogInformation("Completed Plaid data processing for user {UserId}", request.UserId);
            }
            catch (Exception ex)
            {
                //TODO alarm email
                logger.LogError(ex, "Error processing Plaid data for user {UserId}", request.UserId);
            }
        }
    }
}

public class PlaidDataProcessingRequest
{
    public string UserId { get; set; } = string.Empty;
    public string AccessToken { get; set; } = string.Empty;
}

