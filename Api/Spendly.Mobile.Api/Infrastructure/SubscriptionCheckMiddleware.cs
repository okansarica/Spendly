namespace Spendly.Mobile.Api.Infrastructure;

using Microsoft.AspNetCore.Authorization;
using Spendly.Mobile.BusinessLayer.Services.User;
using Spendly.Shared.Localization;
using Spendly.Shared.ViewModels;

public class SubscriptionCheckMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context, UserService userService, RequestContextViewModel requestContextViewModel)
    {
        var endpoint = context.GetEndpoint();
        var authorizeAttribute = endpoint?.Metadata.GetMetadata<AuthorizeAttribute>();
        var allowAnonymousAttribute = endpoint?.Metadata.GetMetadata<IAllowAnonymous>();

        if (authorizeAttribute != null && allowAnonymousAttribute == null)
        {
            if (context.User.Identity?.IsAuthenticated == true)
            {
                var response = await userService.GetSubscriptionEndDateAsync();
                
                if (response.IsSuccess)
                {
                    var endDate = response.Data;
                    
                    if (endDate.HasValue && endDate.Value < DateTime.UtcNow)
                    {
                        context.Response.StatusCode = 400;
                        context.Response.ContentType = "application/json";
                        
                        var errorResponse = new
                        {
                            message = MessageCodes.SubscriptionExpired,
                        };
                        
                        await context.Response.WriteAsJsonAsync(errorResponse);
                        return;
                    }
                }
            }
        }

        await next(context);
    }
}


