namespace Spendly.Mobile.Api.Infrastructure
{
    using Serilog.Context;
    using Spendly.Shared.ViewModels;

    public class SerilogContextEnricherMiddleware(RequestDelegate next)
    {
        public async Task InvokeAsync(HttpContext context)
        {
            // SellerId alma
            string? sellerId = null;
            if (context.RequestServices.GetService(typeof(RequestContextViewModel)) is RequestContextViewModel requestContext)
            {
                sellerId = requestContext.TryToGetUserId();
            }

            // IP alma
            var ip = context.Connection.RemoteIpAddress?.ToString();
            // Path
            var path = context.Request.Path.ToString();
            // HttpMethod
            var method = context.Request.Method;
            // UserAgent
            var userAgent = context.Request.Headers["User-Agent"].ToString();
            // SessionId
            var sessionId = context.Items.TryGetValue("SessionId", out var item) ? item?.ToString() : null;
            // TraceId
            var traceId = context.TraceIdentifier;
            // CorrelationId (header'dan veya yoksa null) 
            var correlationId = context.Request.Headers.ContainsKey("X-Correlation-Id") ? context.Request.Headers["X-Correlation-Id"].ToString() : null;
            // Route template
            var routeData = context.GetRouteData();
            var routeTemplate = routeData?.Values != null ? string.Join('/', routeData.Values.Select(kv => $"{kv.Key}:{kv.Value}")) : null;

            using (LogContext.PushProperty("SellerId", sellerId ?? "Unknown"))
            using (LogContext.PushProperty("IpAddress", ip ?? "Unknown"))
            using (LogContext.PushProperty("Path", path))
            using (LogContext.PushProperty("HttpMethod", method))
            using (LogContext.PushProperty("UserAgent", userAgent))
            using (LogContext.PushProperty("SessionId", sessionId ?? "Unknown"))
            using (LogContext.PushProperty("TraceId", traceId))
            using (LogContext.PushProperty("CorrelationId", correlationId ?? "Unknown"))
            using (LogContext.PushProperty("Route", routeTemplate ?? string.Empty))
            {
                await next(context);
            }
        }
    }
}
