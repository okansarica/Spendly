// CHANGED_BY_AI: 2026-03-04 - Add users controller for profile/password/language/account endpoints and subscription
namespace Spendly.Mobile.Api.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Spendly.Mobile.Api.Infrastructure;
using Spendly.Mobile.BusinessLayer.Services.User;
using Spendly.Mobile.ViewModels.User;

[ApiController]
[Route("api/v1/users")]
[Authorize]
public class UsersController(UserService userService, UserSubscriptionService userSubscriptionService) : ControllerBase
{
    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile()
    {
        var response = await userService.GetProfileAsync();
        if (!response.IsSuccess)
        {
            return this.BadRequestFrom(response);
        }

        return Ok(response.Data);
    }

    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateUserProfileRequestViewModel request)
    {
        var response = await userService.UpdateProfileAsync(request);
        if (!response.IsSuccess)
        {
            return this.BadRequestFrom(response);
        }

        return Ok(response.Data);
    }

    [HttpPut("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequestViewModel request)
    {
        var response = await userService.ChangePasswordAsync(request);
        if (!response.IsSuccess)
        {
            return this.BadRequestFrom(response);
        }

        return Ok(new { success = true });
    }

    [HttpPut("language")]
    public async Task<IActionResult> SetLanguagePreference([FromBody] SetLanguagePreferenceRequestViewModel request)
    {
        var response = await userService.SetLanguagePreferenceAsync(request);
        if (!response.IsSuccess)
        {
            return this.BadRequestFrom(response);
        }

        return Ok(response.Data);
    }

    [HttpDelete("account")]
    public async Task<IActionResult> DeleteAccount()
    {
        var response = await userService.DeleteAccountAsync();
        if (!response.IsSuccess)
        {
            return this.BadRequestFrom(response);
        }

        return Ok(new { success = true });
    }

    [HttpGet("subscription-end")]
    public async Task<IActionResult> GetSubscriptionEndDate()
    {
        var response = await userService.GetSubscriptionEndDateAsync();
        if (!response.IsSuccess)
        {
            return this.BadRequestFrom(response);
        }

        return Ok(new { subscriptionEndDateTime = response.Data });
    }

    [HttpGet("subscription-plans")]
    public async Task<IActionResult> GetSubscriptionPlans()
    {
        var response = await userSubscriptionService.GetSubscriptionPlansAsync();
        if (!response.IsSuccess)
        {
            return this.BadRequestFrom(response);
        }

        return Ok(response.Data);
    }

    [HttpPost("create-payment-url")]
    public async Task<IActionResult> CreatePaymentUrl([FromBody] CreatePaymentUrlRequestViewModel request)
    {
        var response = await userSubscriptionService.CreatePaymentUrlAsync(request);
        if (!response.IsSuccess)
        {
            return this.BadRequestFrom(response);
        }

        return Ok(response.Data);
    }

    [HttpPost("stripe-webhook")]
    [AllowAnonymous]
    public async Task<IActionResult> StripeWebhook()
    {
        var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
        var signature = Request.Headers["Stripe-Signature"].ToString();

        var response = await userSubscriptionService.HandleStripeWebhookAsync(json, signature);
        if (!response.IsSuccess)
        {
            return this.BadRequestFrom(response);
        }

        return Ok();
    }
    
    [HttpPost("firebase-token")]
    [AllowAnonymous]
    public async Task<IActionResult> FirebaseToken([FromBody] SaveFirebaseTokenRequest request)
    {
        var response = await userSubscriptionService.SaveFirebaseToken(request);
        if (!response.IsSuccess)
        {
            return this.BadRequestFrom(response);
        }

        return Ok();
    }
}
