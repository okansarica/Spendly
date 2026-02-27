// filepath: /Users/okan/Documents/Software/Projects/SimplePay/SimplePay/SimplePay.Seller.Api/Infrastructure/ControllerExtensions.cs
namespace Spendly.Mobile.Api.Infrastructure;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Spendly.Shared.ViewModels;
using System;
using System.Linq;

public static class ControllerExtensions
{
	// Creates a standardized BadRequest payload from a non-generic FunctionResponse
	public static BadRequestObjectResult BadRequestFrom(this ControllerBase controller, FunctionResponse result)
	{
		return controller.BadRequest(new
		{
			errors = new[]
			{
				new { message = result.ErrorMessage, parameters = (result.MessageParameters?.Cast<object>().ToArray()) ?? Array.Empty<object>() }
			}
		});
	}
	
	public static BadRequestObjectResult BadRequestFrom<T>(this ControllerBase controller, FunctionResponse<T> result)
	{
		return controller.BadRequest(new
		{
			errors = new[]
			{
				new { message = result.ErrorMessage, parameters = (result.MessageParameters?.Cast<object>().ToArray()) ?? Array.Empty<object>() }
			}
		});
	}
	
	public static IActionResult InvalidModelStateResponse(ActionContext context)
	{
		var errors = context.ModelState
			.Where(x => x.Value?.Errors.Count > 0)
			.SelectMany(x => x.Value?.Errors ?? Enumerable.Empty<ModelError>())
			.Select(error => new { message = error.ErrorMessage, parameters = Array.Empty<object>() })
			.ToList();

		return new BadRequestObjectResult(new { errors });
	}
	
	// public static void AddTokenCookieToResponse(this ControllerBase controller, TokenWrapperViewModel? tokenWrapperViewModel,IWebHostEnvironment env)
	// {
	// 	if (tokenWrapperViewModel == null)
	// 	{
	// 		//This scenario is for sellers who didn't verify their e-mail yet
	// 		return;
	// 	}
	// 	var options = new CookieOptions
	// 	{
	// 		HttpOnly = true,
	// 		Secure = true,
	// 		SameSite = env.IsDevelopment()?SameSiteMode.None: SameSiteMode.Strict,
	// 		Expires = tokenWrapperViewModel.AccessToken!.ExpireDateTime
	// 	};
	// 	controller.Response.Cookies.Append(Constants.AccessTokenName, tokenWrapperViewModel.AccessToken.Token, options);
	//
	// 	if (tokenWrapperViewModel.RefreshToken == null)
	// 	{
	// 		return;
	// 	}
	// 	var refreshTokenOptions = new CookieOptions
	// 	{
	// 		HttpOnly = true,
	// 		Secure = true,
	// 		SameSite = env.IsDevelopment()?SameSiteMode.None: SameSiteMode.Strict,
	// 		Expires = DateTimeOffset.UtcNow.AddDays(60),
	// 		Path = "/seller/refresh-access-token",
	// 		IsEssential = true
	// 	};
	// 	controller.Response.Cookies.Append(Constants.RefreshTokenName, tokenWrapperViewModel.RefreshToken.Token, refreshTokenOptions);
	// }
}
