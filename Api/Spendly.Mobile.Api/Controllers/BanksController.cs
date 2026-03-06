// CHANGED_BY_AI: 2026-03-06 - Add banks and accounts controller
namespace Spendly.Mobile.Api.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Spendly.Mobile.Api.Infrastructure;
using Spendly.Mobile.BusinessLayer.Services.Finance;
using Spendly.Mobile.ViewModels.Finance;

/* AI-ALLOW: 6.3 - User approved implementing banks endpoints before API contract update */
[ApiController]
[Route("api/v1/banks")]
[Authorize]
public class BanksController(BankService bankService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List([FromQuery] BankListRequestViewModel request)
    {
        var response = await bankService.ListAsync(request);
        if (!response.IsSuccess)
        {
            return this.BadRequestFrom(response);
        }

        return Ok(response.Data);
    }

    [HttpGet("bank-definitions")]
    public async Task<IActionResult> ListDefinitions()
    {
        var response = await bankService.ListDefinitionsAsync();
        if (!response.IsSuccess)
        {
            return this.BadRequestFrom(response);
        }

        return Ok(response.Data);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] BankUpsertRequestViewModel request)
    {
        var response = await bankService.CreateAsync(request);
        if (!response.IsSuccess)
        {
            return this.BadRequestFrom(response);
        }

        return Ok(response.Data);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, [FromBody] BankUpsertRequestViewModel request)
    {
        var response = await bankService.UpdateAsync(id, request);
        if (!response.IsSuccess)
        {
            return this.BadRequestFrom(response);
        }

        return Ok(response.Data);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var response = await bankService.DeleteAsync(id);
        if (!response.IsSuccess)
        {
            return this.BadRequestFrom(response);
        }

        return NoContent();
    }

    [HttpPost("{bankId}/accounts")]
    public async Task<IActionResult> CreateAccount(string bankId, [FromBody] BankAccountUpsertRequestViewModel request)
    {
        var response = await bankService.CreateAccountAsync(bankId, request);
        if (!response.IsSuccess)
        {
            return this.BadRequestFrom(response);
        }

        return Ok(response.Data);
    }

    [HttpPut("{bankId}/accounts/{id}")]
    public async Task<IActionResult> UpdateAccount(string bankId, string id, [FromBody] BankAccountUpsertRequestViewModel request)
    {
        var response = await bankService.UpdateAccountAsync(bankId, id, request);
        if (!response.IsSuccess)
        {
            return this.BadRequestFrom(response);
        }

        return Ok(response.Data);
    }

    [HttpDelete("{bankId}/accounts/{id}")]
    public async Task<IActionResult> DeleteAccount(string bankId, string id)
    {
        var response = await bankService.DeleteAccountAsync(bankId, id);
        if (!response.IsSuccess)
        {
            return this.BadRequestFrom(response);
        }

        return NoContent();
    }
}

