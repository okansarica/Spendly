// CHANGED_BY_AI: 2026-03-02 - Add merchant nickname endpoint
// CHANGED_BY_AI: 2026-03-02 - Add merchant delete endpoint
// CHANGED_BY_AI: 2026-03-02 - Add merchants controller
// CHANGED_BY_AI: 2026-03-02 - Add merchant update endpoint
namespace Spendly.Mobile.Api.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using Spendly.Mobile.Api.Infrastructure;
using Spendly.Mobile.BusinessLayer.Services.Finance;
using Spendly.Mobile.ViewModels.Finance;
using Spendly.Shared.Localization;
using Spendly.Shared.ViewModels;

[ApiController]
[Route("api/v1/merchants")]
[Authorize]
public class MerchantsController(MerchantService merchantService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetList([FromQuery] MerchantListRequestViewModel request)
    {
        var response = await merchantService.ListAsync( request);
        if (!response.IsSuccess)
        {
            return this.BadRequestFrom(response);
        }

        return Ok(response.Data);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, [FromBody] MerchantUpdateRequestViewModel request)
    {
        var response = await merchantService.UpdateAsync(id, request);
        if (!response.IsSuccess)
        {
            return this.BadRequestFrom(response);
        }

        return Ok(response.Data);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var response = await merchantService.DeleteAsync(id);
        if (!response.IsSuccess)
        {
            return this.BadRequestFrom(response);
        }

        return NoContent();
    }
}
