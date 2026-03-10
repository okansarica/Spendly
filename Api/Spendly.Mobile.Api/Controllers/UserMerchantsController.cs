// CHANGED_BY_AI: 2026-03-02 - Add merchant nickname endpoint
// CHANGED_BY_AI: 2026-03-02 - Add merchant delete endpoint
// CHANGED_BY_AI: 2026-03-02 - Add merchants controller
// CHANGED_BY_AI: 2026-03-02 - Add merchant update endpoint
namespace Spendly.Mobile.Api.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Spendly.Mobile.Api.Infrastructure;
using Spendly.Mobile.BusinessLayer.Services.Finance;
using Spendly.Mobile.ViewModels.Finance;

[ApiController]
[Route("api/v1/user-merchants")]
[Authorize]
public class UserMerchantsController(UserMerchantService userMerchantService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetList([FromQuery] MerchantListRequestViewModel request)
    {
        var response = await userMerchantService.ListAsync( request);
        if (!response.IsSuccess)
        {
            return this.BadRequestFrom(response);
        }

        return Ok(response.Data);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, [FromBody] UserMerchantUpdateRequestViewModel request)
    {
        var response = await userMerchantService.UpdateAsync(id, request);
        if (!response.IsSuccess)
        {
            return this.BadRequestFrom(response);
        }

        return Ok(response.Data);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var response = await userMerchantService.DeleteAsync(id);
        if (!response.IsSuccess)
        {
            return this.BadRequestFrom(response);
        }

        return NoContent();
    }
}
