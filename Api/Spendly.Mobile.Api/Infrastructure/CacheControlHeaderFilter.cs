namespace Spendly.Mobile.Api.Infrastructure
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Filters;

    public class CacheControlHeaderFilter : IActionFilter
    {
        public void OnActionExecuting(ActionExecutingContext context)
        {
            // No-op
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            var httpContext = context.HttpContext;
            if (httpContext.Request.Method == "GET" &&
                context.Result is ObjectResult objectResult &&
                objectResult.StatusCode == 200 &&
                !httpContext.Response.Headers.ContainsKey("Cache-Control"))
            {
                httpContext.Response.Headers["Cache-Control"] = "public, max-age=60";
            }
        }
    }
}

