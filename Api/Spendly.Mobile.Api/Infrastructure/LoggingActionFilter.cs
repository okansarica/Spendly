namespace Spendly.Mobile.Api.Infrastructure;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Serilog;

public class LoggingActionFilter : IAsyncActionFilter
{
	public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
	{
		// Ortak metadata: route + action name
		var action = context.ActionDescriptor.DisplayName;
		var route = context.HttpContext.Request.Path.Value ?? string.Empty;
		var method = context.HttpContext.Request.Method;

		// Argümanları dictionary olarak al (reflection yok)
		var args = context.ActionArguments;

        var sid = RequestSession.Get(context.HttpContext) ?? string.Empty;
		var logger = Log.ForContext("Action", action)
			.ForContext("Route", route)
			.ForContext("HttpMethod", method)
            .ForContext("SessionId", sid);

		logger.Debug("HTTP {Method} {Route} -> {Action} args {@Args}", 
			method, route, action, args);

		var executed = await next();

		if (executed.Exception is { } ex && !executed.ExceptionHandled)
		{
			logger.Error(ex, "HTTP {Method} {Route} -> {Action} threw", 
				method, route, action);
			return;
		}

		// Return değerini minimal kontrol ile al
		object? returnPayload = executed.Result switch
		{
			ObjectResult obj => obj.Value,
			JsonResult json => json.Value,
			ContentResult content => content.Content,
			_ => executed.Result?.GetType().Name
		};

		logger.Debug("HTTP {Status} {Method} {Route} -> {Action} result {@Result}",
			context.HttpContext.Response.StatusCode,
			method,
			route,
			action,
			returnPayload);
	}
}
