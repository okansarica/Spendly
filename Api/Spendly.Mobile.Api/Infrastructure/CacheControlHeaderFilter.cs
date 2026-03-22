namespace Spendly.Mobile.Api.Infrastructure
{
    using Microsoft.AspNetCore.Mvc.Filters;

    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false)]
    public class CacheControlAttribute(int maxAge = 60) : ActionFilterAttribute
    {

        public override void OnActionExecuted(ActionExecutedContext context)
        {
            if (!context.HttpContext.Response.Headers.ContainsKey("Cache-Control"))
            {
                context.HttpContext.Response.Headers["Cache-Control"] = $"public, max-age={maxAge}";
            }
        }
    }
}

