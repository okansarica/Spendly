namespace Spendly.Mobile.Api.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using Spendly.Mobile.Api.Infrastructure;
using Spendly.Mobile.BusinessLayer.Services.Homepage;
using Spendly.Shared.ViewModels;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class HomepageController(HomepageService homepageService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetHomepage()
    {
        var userIdClaim = User.FindFirst("sub")?.Value ?? User.FindFirst("userId")?.Value;
        
        if (string.IsNullOrEmpty(userIdClaim) || !ObjectId.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        var response = await homepageService.GetHomepageAsync(userId);
        
        if (!response.IsSuccess)
            return this.BadRequestFrom(response);
            
        return Ok(response.Data);
    }
}

