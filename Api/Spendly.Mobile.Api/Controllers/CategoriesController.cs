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
public class CategoriesController(CategoryService categoryService, RequestContextViewModel requestContext) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetList([FromQuery] CategoryListRequestViewModel request)
    {
        var userIdValue = requestContext.TryToGetUserId();
        if (string.IsNullOrWhiteSpace(userIdValue))
        {
            return this.BadRequestFrom(FunctionResponse.Failure(MessageCodes.UserNotFound));
        }

        var response = await categoryService.GetListAsync(ObjectId.Parse(userIdValue), request);
        if (!response.IsSuccess)
        {
            return this.BadRequestFrom(response);
        }

        return Ok(response.Data);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CategoryUpsertRequestViewModel request)
    {
        var userIdValue = requestContext.TryToGetUserId();
        if (string.IsNullOrWhiteSpace(userIdValue))
        {
            return this.BadRequestFrom(FunctionResponse.Failure(MessageCodes.UserNotFound));
        }

        var response = await categoryService.CreateAsync(ObjectId.Parse(userIdValue), request);
        if (!response.IsSuccess)
        {
            return this.BadRequestFrom(response);
        }

        return Ok(response.Data);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, [FromBody] CategoryUpsertRequestViewModel request)
    {
        var userIdValue = requestContext.TryToGetUserId();
        if (string.IsNullOrWhiteSpace(userIdValue))
        {
            return this.BadRequestFrom(FunctionResponse.Failure(MessageCodes.UserNotFound));
        }

        var response = await categoryService.UpdateAsync(ObjectId.Parse(userIdValue), id, request);
        if (!response.IsSuccess)
        {
            return this.BadRequestFrom(response);
        }

        return Ok(response.Data);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var userIdValue = requestContext.TryToGetUserId();
        if (string.IsNullOrWhiteSpace(userIdValue))
        {
            return this.BadRequestFrom(FunctionResponse.Failure(MessageCodes.UserNotFound));
        }

        var response = await categoryService.DeleteAsync(ObjectId.Parse(userIdValue), id);
        if (!response.IsSuccess)
        {
            return this.BadRequestFrom(response);
        }

        return Ok();
    }

    [HttpGet("{id}/merchants")]
    public async Task<IActionResult> GetMerchants(string id)
    {
        var userIdValue = requestContext.TryToGetUserId();
        if (string.IsNullOrWhiteSpace(userIdValue))
        {
            return this.BadRequestFrom(FunctionResponse.Failure(MessageCodes.UserNotFound));
        }

        var response = await categoryService.GetMerchantsAsync(ObjectId.Parse(userIdValue), id);
        if (!response.IsSuccess)
        {
            return this.BadRequestFrom(response);
        }

        return Ok(response.Data);
    }

    [HttpPost("{id}/merchants")]
    public async Task<IActionResult> AddMerchants(string id, [FromBody] CategoryMerchantsRequestViewModel request)
    {
        var userIdValue = requestContext.TryToGetUserId();
        if (string.IsNullOrWhiteSpace(userIdValue))
        {
            return this.BadRequestFrom(FunctionResponse.Failure(MessageCodes.UserNotFound));
        }

        var response = await categoryService.AddMerchantsAsync(ObjectId.Parse(userIdValue), id, request);
        if (!response.IsSuccess)
        {
            return this.BadRequestFrom(response);
        }

        return Ok(response.Data);
    }

    [HttpDelete("{id}/merchants/{merchantId}")]
    public async Task<IActionResult> RemoveMerchant(string id, string merchantId)
    {
        var userIdValue = requestContext.TryToGetUserId();
        if (string.IsNullOrWhiteSpace(userIdValue))
        {
            return this.BadRequestFrom(FunctionResponse.Failure(MessageCodes.UserNotFound));
        }

        var response = await categoryService.RemoveMerchantAsync(ObjectId.Parse(userIdValue), id, merchantId);
        if (!response.IsSuccess)
        {
            return this.BadRequestFrom(response);
        }

        return Ok();
    }
}

