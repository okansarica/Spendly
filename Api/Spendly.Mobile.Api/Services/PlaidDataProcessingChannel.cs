namespace Spendly.Mobile.Api.Services;

using System.Threading.Channels;

public class PlaidDataProcessingChannel
{
    private readonly Channel<PlaidDataProcessingBAcgorundServiceRequest> _channel;

    public PlaidDataProcessingChannel()
    {
        _channel = Channel.CreateUnbounded<PlaidDataProcessingBAcgorundServiceRequest>();
    }

    public async Task EnqueueAsync(PlaidDataProcessingBAcgorundServiceRequest bAcgorundServiceRequest)
    {
        await _channel.Writer.WriteAsync(bAcgorundServiceRequest);
    }

    public Channel<PlaidDataProcessingBAcgorundServiceRequest> GetChannel() => _channel;
}

