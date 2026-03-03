// CHANGED_BY_AI: 2026-03-02 - Add reports controller
namespace Spendly.Mobile.Api.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using Spendly.Mobile.Api.Infrastructure;
using Spendly.Mobile.BusinessLayer.Services.Reports;
using Spendly.Mobile.ViewModels.Reports;
using Spendly.Shared.ViewModels;

[ApiController]
[Route("api/v1/reports")]
[Authorize]
public class ReportsController(ReportsService reportsService) : ControllerBase
{
    [HttpGet("overview")]
    public async Task<IActionResult> GetOverview([FromQuery] ReportsOverviewRequestViewModel request)
    {
        var response = await reportsService.GetOverviewAsync(request);
        if (!response.IsSuccess)
        {
            return this.BadRequestFrom(response);
        }
        return Ok(response.Data);
    }

    [HttpGet("category/{categoryId}")]
    public async Task<IActionResult> GetCategoryDetail(string categoryId, [FromQuery] ReportsCategoryRequestViewModel request)
    {
        var response = await reportsService.GetCategoryDetailAsync(categoryId, request);
        if (!response.IsSuccess)
        {
            return this.BadRequestFrom(response);
        }
        return Ok(response.Data);
    }

    [HttpGet("accounts/overview")]
    public async Task<IActionResult> GetAccountsOverview([FromQuery] AccountsOverviewRequestViewModel request)
    {
        var response = await reportsService.GetAccountsOverviewAsync(request);
        if (!response.IsSuccess)
        {
            return this.BadRequestFrom(response);
        }
        return Ok(response.Data);
    }

    [HttpGet("accounts/{accountId}")]
    public async Task<IActionResult> GetAccountDetail(string accountId, [FromQuery] AccountDetailRequestViewModel request)
    {
        var response = await reportsService.GetAccountDetailAsync(accountId, request);
        if (!response.IsSuccess)
        {
            return this.BadRequestFrom(response);
        }
        return Ok(response.Data);
    }
}
