namespace Spendly.Shared.Core.Interception;

using Spendly.Shared.ViewModels;
using System.Reflection;

public static class CacheKeyHelper
{
	public static string BuildKey(CacheableAttribute attr, MethodInfo method, RequestContextViewModel? ctx)
	{
		return BuildCore( method.DeclaringType?.FullName, method.Name, ctx, attr.IncludeUserIdInKey);
	}
	public static string BuildFor<TService>(string methodName, RequestContextViewModel? requestContextViewModel, bool includeSeller = true)
	{
		return BuildCore(typeof(TService).FullName, methodName, requestContextViewModel, includeSeller);
	}
	private static string BuildCore(string? typeFullName, string methodName, RequestContextViewModel? ctx, bool includeSeller)
	{
		var key = $"{typeFullName}.{methodName}";
		if (includeSeller && ctx != null)
		{
			var userId = ctx.TryToGetUserId();
			if (!string.IsNullOrWhiteSpace(userId))
			{
				key += $":user:{userId}";
			}
		}
		return key;
	}
}
