namespace Spendly.Mobile.Api.Controllers;

using BusinessLayer.Services.Plaid;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Core;
using Shared.ViewModels.Plaid;
using Spendly.Mobile.Api.Infrastructure;
using Spendly.Mobile.Api.Services;
using Spendly.Mobile.BusinessLayer.Services.Finance;
using ViewModels.Plaid;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class PlaidController(
    PlaidService plaidService,
    PlaidDataProcessingChannel plaidDataProcessingChannel) : ControllerBase
{

    [HttpPost("create-link-token")]
    public async Task<IActionResult> CreateLinkToken()
    {
        var token = await plaidService.CreateLinkTokenAsync();
        return Ok(new { linkToken = token });
    }


    [HttpPost("complete-integration")]
    public async Task<IActionResult> CompleteIntegration([FromBody] CompleteIntegrationRequestViewModel completeIntegrationRequestViewModel)
    {
        var completeResponse = await plaidService.CompleteIntegration(completeIntegrationRequestViewModel);
        if (!completeResponse.IsSuccess)
        {
            return this.BadRequestFrom(completeResponse);
        }

        await plaidDataProcessingChannel.EnqueueAsync(new PlaidDataProcessingBackgroundServiceRequestViewModel
        {
            UserId = User.Identity!.Name!,
            AccessToken = completeResponse.Data!.AccessToken,
            BankId = completeResponse.Data!.BankId,
            //NewAccountIds = completeResponse.Data!.NewAccountPlaidIds
        });
        
        return Ok();
    }
}

