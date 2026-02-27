namespace Spendly.Shared.Core.Interception;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
public sealed class CacheableAttribute : Attribute
{
    /// <summary>
    /// Absolute expiration in seconds; if <= 0, a default (4 hours) will be used by the interceptor.
    /// </summary>
    public int DurationSeconds { get; init; }

    /// <summary>
    /// If true, and RequestContext is available, include SellerId in cache key. Default true.
    /// </summary>
    public bool IncludeSellerIdInKey { get; init; } = true;
}
