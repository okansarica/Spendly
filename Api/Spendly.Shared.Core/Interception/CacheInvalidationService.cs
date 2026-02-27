namespace Spendly.Shared.Core.Interception;

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using ViewModels;

public interface ICacheInvalidationService
{
	void Invalidate<TService>(string methodName, bool includeSeller = true);
}

public class CacheInvalidationService(IMemoryCache cache, ILogger<CacheInvalidationService> logger, RequestContextViewModel ctx) : ICacheInvalidationService
{
	private readonly IMemoryCache _cache = cache;
	private readonly ILogger<CacheInvalidationService> _logger = logger;
	private readonly RequestContextViewModel _ctx = ctx;

	public void Invalidate<TService>(string methodName, bool includeSeller = true)
	{
		var key = CacheKeyHelper.BuildFor<TService>(methodName, _ctx, includeSeller);
		_cache.Remove(key);
		_logger.LogDebug("[Cache INVALIDATE] {Key}", key);
	}
}
