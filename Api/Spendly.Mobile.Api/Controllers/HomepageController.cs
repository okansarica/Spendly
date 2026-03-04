namespace Spendly.Mobile.Api.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Spendly.Mobile.Api.Infrastructure;
using Spendly.Mobile.BusinessLayer.Services.Homepage;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class HomepageController(HomepageService homepageService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetHomepage()
    {
        var response = await homepageService.GetHomepageAsync();
        
        if (!response.IsSuccess)
            return this.BadRequestFrom(response);
            
        return Ok(response.Data);
    }
}
