namespace Spendly.Mobile.Api.Infrastructure;

using System.Security.Claims;
using Spendly.Shared.ViewModels;

public class RequestContextMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context, RequestContextViewModel requestContextViewModel)
    {
        var idClaim = context.User.Claims.SingleOrDefault(p => p.Type == ClaimTypes.Name);

        if (idClaim == null)
        {
            if (context.Request.Headers.TryGetValue("X-Test-UserId", out var val))
            {
                requestContextViewModel.SetUserId(val.ToString());
            }
            else
            {
                requestContextViewModel.SetUserId(null);
            }
        }
        else
        {
            requestContextViewModel.SetUserId(idClaim.Value);
        }
        
        if (context.Request.Headers.TryGetValue("X-Timezone", out var timezone))
        {
            requestContextViewModel.Timezone = timezone.ToString();
        }
        
        if (context.Request.Headers.TryGetValue("X-session-id", out var sessionId))
        {
            requestContextViewModel.SessionId = Guid.Parse(sessionId.ToString());
        }
        
        if (context.Request.Headers.TryGetValue("X-language", out var language))
        {
            requestContextViewModel.Language = language.ToString();
        }

        await next(context);
    }
}

