namespace Spendly.Mobile.Api.Controllers;

using Microsoft.AspNetCore.Mvc;
using Spendly.Mobile.Api.Infrastructure;
using Spendly.Mobile.BusinessLayer.Services.Auth;
using Spendly.Mobile.ViewModels.Auth;

[ApiController]
[Route("api/v1/[controller]")]
public class AuthController(AuthService authService) : ControllerBase
{
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestViewModel request)
    {
        var response = await authService.LoginAsync(request);
        if (!response.IsSuccess)
            return this.BadRequestFrom(response);
        return Ok(response.Data);
    }

    [HttpPost("social-login")]
    public async Task<IActionResult> SocialLogin([FromBody] SocialLoginRequestViewModel request)
    {
        var response = await authService.SocialLoginAsync(request);
        if (!response.IsSuccess)
            return this.BadRequestFrom(response);
        return Ok(response.Data);
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestViewModel request)
    {
        var response = await authService.ForgotPasswordAsync(request);
        if (!response.IsSuccess)
            return this.BadRequestFrom(response);
        return Ok(new { success = true });
    }
    
    //TODO register endpointi eklenmemis
}

