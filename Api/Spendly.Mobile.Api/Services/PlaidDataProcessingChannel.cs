namespace Spendly.Mobile.Api.Services;

using System.Threading.Channels;

public class PlaidDataProcessingChannel
{
    private readonly Channel<PlaidDataProcessingRequest> _channel;

    public PlaidDataProcessingChannel()
    {
        _channel = Channel.CreateUnbounded<PlaidDataProcessingRequest>();
    }

    public async Task EnqueueAsync(PlaidDataProcessingRequest request)
    {
        await _channel.Writer.WriteAsync(request);
    }

    public Channel<PlaidDataProcessingRequest> GetChannel() => _channel;
}

