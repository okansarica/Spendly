namespace Spendly.Mobile.Api.Services;

using Shared.ViewModels.Plaid;
using System.Threading.Channels;
using ViewModels.Plaid;

public class PlaidDataProcessingChannel
{
    private readonly Channel<PlaidDataProcessingBackgroundServiceRequestViewModel> _channel = Channel.CreateUnbounded<PlaidDataProcessingBackgroundServiceRequestViewModel>();

    public async Task EnqueueAsync(PlaidDataProcessingBackgroundServiceRequestViewModel backgroundServiceRequestViewModel)
    {
        await _channel.Writer.WriteAsync(backgroundServiceRequestViewModel);
    }

    public Channel<PlaidDataProcessingBackgroundServiceRequestViewModel> GetChannel() => _channel;
}

