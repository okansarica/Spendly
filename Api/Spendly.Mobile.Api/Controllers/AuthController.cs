namespace Spendly.Mobile.Api.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Spendly.Mobile.Api.Infrastructure;
using Spendly.Mobile.BusinessLayer.Services.Auth;
using Spendly.Mobile.ViewModels.Auth;

[ApiController]
[Route("api/v1/[controller]")]
[AllowAnonymous]
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

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequestViewModel request)
    {
        var response = await authService.RegisterAsync(request);
        if (!response.IsSuccess)
            return this.BadRequestFrom(response);
        return Ok(response.Data);
    }

    [HttpPost("refresh-access-token")]
    public async Task<IActionResult> RefreshAccessToken([FromBody] RefreshTokenRequestViewModel request)
    {
        var response = await authService.RefreshAccessTokenAsync(request);
        if (!response.IsSuccess)
            return this.BadRequestFrom(response);
        return Ok(response.Data);
    }

    [HttpPost("verify-email")]
    public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailRequestViewModel request)
    {
        var response = await authService.VerifyEmailAsync(request);
        if (!response.IsSuccess)
            return this.BadRequestFrom(response);
        return Ok(response.Data);
    }

    [HttpPost("resend-verification")]
    public async Task<IActionResult> ResendVerification([FromBody] ResendCodeRequestViewModel request)
    {
        var response = await authService.ResendCodeAsync(request);
        if (!response.IsSuccess)
            return this.BadRequestFrom(response);
        return Ok(new { success = true });
    }
}
