namespace Spendly.Mobile.Api.Infrastructure;

public class GlobalExceptionLoggingMiddleware(RequestDelegate next, ILogger<GlobalExceptionLoggingMiddleware> logger)
{
	public async Task Invoke(HttpContext ctx)
	{
		try
		{
			await next(ctx);
		}
		catch(Exception ex)
		{
			// enrich with request details
			var id = ctx.TraceIdentifier;
            var sid = RequestSession.Get(ctx) ?? string.Empty;
			logger.LogError(ex, "Unhandled exception. TraceId: {TraceId} SessionId: {SessionId} Path: {Path}", id, sid, ctx.Request.Path);
			ctx.Response.StatusCode = 500;
			// response body (standard) + include exception message to aid tests
			await ctx.Response.WriteAsJsonAsync(new { error = "InternalServerError", traceId = id, sessionId = sid });
		}
	}
}
