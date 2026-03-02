// CHANGED_BY_AI: 2026-03-02 - Add merchants controller
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
public class MerchantsController(MerchantService merchantService, RequestContextViewModel requestContext) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetList([FromQuery] MerchantListRequestViewModel request)
    {
        var userIdValue = requestContext.TryToGetUserId();
        if (string.IsNullOrWhiteSpace(userIdValue))
        {
            return this.BadRequestFrom(FunctionResponse.Failure(MessageCodes.UserNotFound));
        }

        var response = await merchantService.GetListAsync(ObjectId.Parse(userIdValue), request);
        if (!response.IsSuccess)
        {
            return this.BadRequestFrom(response);
        }

        return Ok(response.Data);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetDetail(string id, [FromQuery] MerchantListRequestViewModel request)
    {
        var userIdValue = requestContext.TryToGetUserId();
        if (string.IsNullOrWhiteSpace(userIdValue))
        {
            return this.BadRequestFrom(FunctionResponse.Failure(MessageCodes.UserNotFound));
        }

        var response = await merchantService.GetDetailAsync(ObjectId.Parse(userIdValue), id, request);
        if (!response.IsSuccess)
        {
            return this.BadRequestFrom(response);
        }

        return Ok(response.Data);
    }

    [HttpPut("{id}/category")]
    public async Task<IActionResult> UpdateCategory(string id, [FromBody] MerchantCategoryUpdateRequestViewModel request)
    {
        var userIdValue = requestContext.TryToGetUserId();
        if (string.IsNullOrWhiteSpace(userIdValue))
        {
            return this.BadRequestFrom(FunctionResponse.Failure(MessageCodes.UserNotFound));
        }

        var response = await merchantService.UpdateCategoryAsync(ObjectId.Parse(userIdValue), id, request);
        if (!response.IsSuccess)
        {
            return this.BadRequestFrom(response);
        }

        return Ok(response.Data);
    }
}

