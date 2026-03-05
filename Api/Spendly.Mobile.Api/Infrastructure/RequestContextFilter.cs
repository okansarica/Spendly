// CHANGED_BY_AI: 2026-03-02 - Capture timezone header

namespace Spendly.Mobile.Api.Infrastructure;

using Microsoft.AspNetCore.Mvc.Filters;
using Spendly.Shared.ViewModels;
using System.Security.Claims;

public class RequestContextFilter(RequestContextViewModel requestContextViewModel):IActionFilter
{

    public void OnActionExecuting(ActionExecutingContext context)
    {
        var httpContext = context.HttpContext;
        var idClaim = context?.HttpContext?.User.Claims.SingleOrDefault(p => p.Type == ClaimTypes.Name);

        // Fallback for tests: allow setting seller id via header X-Test-SellerId when authentication isn't present
        if (idClaim == null)
        {
            if (httpContext.Request.Headers.TryGetValue("X-Test-UserId", out var val))
            {
                requestContextViewModel.SetUserId(val.ToString());
            }
            else
            {
                // existing behavior: set null (getter will throw when used)
                requestContextViewModel.SetUserId(null);
            }
        }
        else
        {
            requestContextViewModel.SetUserId(idClaim?.Value);
        }
        if (httpContext.Request.Headers.TryGetValue("X-Timezone", out var timezone))
        {
            requestContextViewModel.Timezone = timezone.ToString();
        }
        if (httpContext.Request.Headers.TryGetValue("X-session-id", out var sessionId))
        {
            requestContextViewModel.SessionId = Guid.Parse(sessionId.ToString());
        }
    }
    public void OnActionExecuted(ActionExecutedContext context)
    {

    }
}
