namespace Spendly.Mobile.Api.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Spendly.Mobile.Api.Infrastructure;
using Spendly.Mobile.Api.Services;
using Spendly.Mobile.BusinessLayer.Services.Finance;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class PlaidController(
    PlaidService plaid,
    BankService bankService) : ControllerBase
{

    [HttpPost("create-link-token")]
    public async Task<IActionResult> CreateLinkToken()
    {
        var token = await plaid.CreateLinkTokenAsync();
        return Ok(new { linkToken = token });
    }

    //TODO move to view models and sent to business service
    public class ExchangeRequestViewModel { public string PublicToken { get; set; } = string.Empty; }

    [HttpPost("exchange-public-token")]
    public async Task<IActionResult> CompleteIntegration([FromBody] ExchangeRequestViewModel exchangeRequestViewModel)
    {
        var completeResponse = await plaid.CompleteIntegration(exchangeRequestViewModel.PublicToken);
        if (!completeResponse.IsSuccess)
        {
            return this.BadRequestFrom(completeResponse);
        }
        return Ok();
    }
}

