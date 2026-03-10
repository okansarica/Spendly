namespace Spendly.Mobile.Api.Controllers;

using BusinessLayer.Services.Plaid;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Shared.Core;
using Shared.ViewModels.Plaid;
using Spendly.Mobile.Api.Infrastructure;
using Spendly.Mobile.Api.Services;
using Spendly.Mobile.BusinessLayer.Services.Finance;
using System.Diagnostics;
using ViewModels.Plaid;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class PlaidController(
    PlaidService plaidService,
    BankService bankService,
    IDataProtectionProvider dataProtectionProvider,
    PlaidDataProcessingChannel plaidDataProcessingChannel) : ControllerBase
{
    private readonly IDataProtector _protector =
        dataProtectionProvider.CreateProtector("UserPlaidTokenProtector");
    
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
        Debug.WriteLine(DateTime.Now+" Complete integrastion finished");
        return Ok();
    }

    [HttpPost("webhook/historical-update")]
    [AllowAnonymous]
    public async Task<IActionResult> HistoricalUpdate([FromBody] HistoricalUpdateWebhookViewModel request)
    {
        //TODO idempotency
        //TODO verify webhook
        //TODO check type etc
        if (request.WebhookType!="HISTORICAL_UPDATE")
        {
            return Ok();
        }
        
        //TODO kaydet
        
        var response = await bankService.GetBankAndUserPlaidTokenWithItemId(request.ItemId);
        
        var accessToken = _protector.Unprotect(response.userPlaidToken.EncryptedAccessToken);
        
        await plaidDataProcessingChannel.EnqueueAsync(new PlaidDataProcessingBackgroundServiceRequestViewModel
        {
            UserId = response.bank.UserId.ToString(),
            AccessToken = accessToken,
            BankId = response.bank.Id.ToString(),
            //NewAccountIds = completeResponse.Data!.NewAccountPlaidIds
        });
        
        return Ok();
    }
}

