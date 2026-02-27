
namespace Spendly.Mobile.Api.Infrastructure;

using Serilog.Context;

public static class RequestSession
{
	public const string ItemKey = "SessionId";
	public const string HeaderName = "X-Session-Id";

	public static string? Get(HttpContext ctx)
		=> ctx.Items.TryGetValue(ItemKey, out var v) ? v as string : null;
}

public class RequestSessionMiddleware(RequestDelegate next)
{

	public async Task InvokeAsync(HttpContext context)
	{
		// Prefer inbound header if provided, otherwise generate a new one
		var inbound = context.Request.Headers[RequestSession.HeaderName].FirstOrDefault();
		var sessionId = !string.IsNullOrWhiteSpace(inbound)
			? inbound!.Trim()
			: CreateSessionId();

		// Store on HttpContext for downstream access and set response header
		context.Items[RequestSession.ItemKey] = sessionId;
		context.Response.OnStarting(() =>
		{
			context.Response.Headers[RequestSession.HeaderName] = sessionId;
			return Task.CompletedTask;
		});

		// Push into Serilog's ambient LogContext so all logs within the request include it
		using (LogContext.PushProperty("SessionId", sessionId))
		{
			await next(context);
		}
	}

	private static string CreateSessionId()
	{
#if NET9_0_OR_GREATER
		return Guid.CreateVersion7().ToString("N");
#else
        return Guid.NewGuid().ToString("N");
#endif
	}
}

