// CHANGED_BY_AI: 2026-03-02 - Add categories controller
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
[Route("api/v1/categories")]
[Authorize]
public class CategoriesController(UserCategoryService userCategoryService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List([FromQuery] CategoryListRequestViewModel request)
    {
        var response = await userCategoryService.GetListAsync(request);
        if (!response.IsSuccess)
        {
            return this.BadRequestFrom(response);
        }

        return Ok(response.Data);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CategoryUpsertRequestViewModel request)
    {
        var response = await userCategoryService.CreateAsync(request);
        if (!response.IsSuccess)
        {
            return this.BadRequestFrom(response);
        }

        return Ok(response.Data);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, [FromBody] CategoryUpsertRequestViewModel request)
    {
        var response = await userCategoryService.UpdateAsync(id, request);
        if (!response.IsSuccess)
        {
            return this.BadRequestFrom(response);
        }

        return Ok(response.Data);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var response = await userCategoryService.DeleteAsync(id);
        if (!response.IsSuccess)
        {
            return this.BadRequestFrom(response);
        }

        return Ok();
    }

    [HttpGet("{id}/merchants")]
    public async Task<IActionResult> GetMerchants(string id)
    {
        var response = await userCategoryService.GetMerchantsAsync(id);
        if (!response.IsSuccess)
        {
            return this.BadRequestFrom(response);
        }
    
        return Ok(response.Data);
    }
    
}

