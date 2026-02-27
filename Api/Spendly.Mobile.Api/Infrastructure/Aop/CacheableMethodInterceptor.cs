namespace Spendly.Mobile.Api.Infrastructure.Aop;

using AspectCore.DynamicProxy;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Spendly.Shared.Core.Interception;
using Spendly.Shared.ViewModels;
using System.Reflection;

public class CacheableMethodInterceptor : IInterceptor
{
	public bool AllowMultiple { get; }
	public bool Inherited { get; set; }
	public int Order { get; set; }

	public async Task Invoke(AspectContext context, AspectDelegate next)
	{
		var method = context.ImplementationMethod ?? context.ServiceMethod;
		if (method == null)
		{
			await next(context);
			return;
		}

		// Resolve dependencies lazily
		var sp = context.ServiceProvider;
		var cache = (IMemoryCache?)sp.GetService(typeof(IMemoryCache));
		var logger = (ILogger<CacheableMethodInterceptor>?)sp.GetService(typeof(ILogger<CacheableMethodInterceptor>))
				  ?? NullLogger<CacheableMethodInterceptor>.Instance;
		var requestContext = (RequestContextViewModel?)sp.GetService(typeof(RequestContextViewModel));

		var attr = method.GetCustomAttribute<CacheableAttribute>(inherit: true)
				   ?? method.DeclaringType?.GetCustomAttribute<CacheableAttribute>(inherit: true);
		if (attr == null)
		{
			await next(context);
			return;
		}

		var durationSeconds = attr.DurationSeconds > 0 ? attr.DurationSeconds : (int)TimeSpan.FromHours(4).TotalSeconds;
		if (durationSeconds <= 0 || cache is null)
		{
			await next(context);
			return;
		}

		var key = CacheKeyHelper.BuildKey(attr, (MethodInfo)method, requestContext);
		var returnType = ((MethodInfo)method).ReturnType;
		var isTask = typeof(Task).IsAssignableFrom(returnType);
		var isTaskOfT = isTask && returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(Task<>);

		// Non-generic Task is not cached (no return value)
		if (isTask && !isTaskOfT)
		{
			await next(context);
			return;
		}

		if (cache.TryGetValue(key, out var cachedObject))
		{
			logger.LogDebug("[Cache HIT] {Key}", key);
			if (isTaskOfT)
			{
				var innerType = returnType.GetGenericArguments()[0];
				context.ReturnValue = CreateTaskFromResult(innerType, cachedObject);
			}
			else
			{
				context.ReturnValue = cachedObject;
			}
			return;
		}

		logger.LogDebug("[Cache MISS] {Key}", key);
		await next(context);

		if (isTaskOfT)
		{
			// Await the Task<T> to get the result value
			var task = (Task)context.ReturnValue!;
			await task.ConfigureAwait(false);
			var innerType = returnType.GetGenericArguments()[0];
			var resultObj = GetTaskResult(task);
			if (resultObj != null)
			{
				SetCache(cache, key, resultObj, durationSeconds);
				// Replace with a fresh completed Task<T> holding cached result
				context.ReturnValue = CreateTaskFromResult(innerType, resultObj);
			}
		}
		else
		{
			// Non-task return; cache the value directly
			var resultObj = context.ReturnValue;
			if (resultObj != null)
			{
				SetCache(cache, key, resultObj, durationSeconds);
			}
		}
	}

	private static object? GetTaskResult(Task task)
	{
		var prop = task.GetType().GetProperty("Result", BindingFlags.Instance | BindingFlags.Public);
		return prop?.GetValue(task);
	}

	private static object CreateTaskFromResult(Type innerType, object? result)
	{
		var mi = typeof(Task).GetMethods(BindingFlags.Public | BindingFlags.Static)
			.First(m => m.Name == nameof(Task.FromResult) && m.IsGenericMethodDefinition);
		var g = mi.MakeGenericMethod(innerType);
		return g.Invoke(null, new[] { result! })!;
	}

	private static void SetCache(IMemoryCache cache, string key, object value, int durationSeconds)
	{
		cache.Set(key, value, new MemoryCacheEntryOptions
		{
			AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(durationSeconds)
		});
	}
}
