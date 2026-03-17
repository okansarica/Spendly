namespace Spendly.Mobile.Api.Services;

using Shared.BusinessLayer;
using Shared.ViewModels.Plaid;
using System.Threading.Channels;

public class PlaidDataProcessorHostedService(
    Channel<PlaidDataProcessingBackgroundServiceRequestViewModel> channel,
    IServiceScopeFactory serviceScopeFactory,
    ILogger<PlaidDataProcessorHostedService> logger)
    : BackgroundService
{

    protected async override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var request in channel.Reader.ReadAllAsync(stoppingToken))
        {
            logger.LogInformation("Started Plaid data waiting before processing for user {UserId}", request.UserId);
            await Task.Delay(30000, stoppingToken);
            try
            {
                logger.LogInformation("Starting Plaid data processing for user {UserId}", request.UserId);
                
                using var scope = serviceScopeFactory.CreateScope();

                var sharedPlaidService = scope.ServiceProvider.GetRequiredService<SharedPlaidService>();
                await sharedPlaidService.TransferTransactionsFromPlaidAsync(
                    DateOnly.FromDateTime(DateTime.Today.AddDays(-90)), // son gun dahil degil, bitis tarihi dahil, bugunun kayitlari gece cekilecek onlari cekme
                    DateOnly.FromDateTime(DateTime.Today.AddDays(-1)),
                    request
                );
                
                
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
