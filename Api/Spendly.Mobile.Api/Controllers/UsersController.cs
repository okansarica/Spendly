// CHANGED_BY_AI: 2026-03-03 - Add users controller for profile/password/language/account endpoints
namespace Spendly.Mobile.Api.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Spendly.Mobile.Api.Infrastructure;
using Spendly.Mobile.BusinessLayer.Services.User;
using Spendly.Mobile.ViewModels.User;

[ApiController]
[Route("api/v1/users")]
[Authorize]
public class UsersController(UserService userService) : ControllerBase
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
}
