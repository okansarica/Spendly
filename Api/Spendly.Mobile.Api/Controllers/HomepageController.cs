namespace Spendly.Mobile.Api.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using Spendly.Mobile.Api.Infrastructure;
using Spendly.Mobile.BusinessLayer.Services.Homepage;
using Spendly.Shared.ViewModels;
using System.Security.Claims;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class HomepageController(HomepageService homepageService, RequestContextViewModel requestContext) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetHomepage()
    {
        var userId = ObjectId.Parse(requestContext.UserId);
        var response = await homepageService.GetHomepageAsync(userId);
        
        if (!response.IsSuccess)
            return this.BadRequestFrom(response);
            
        return Ok(response.Data);
    }
}

