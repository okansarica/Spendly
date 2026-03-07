namespace Spendly.Mobile.Api.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Spendly.Mobile.Api.Services;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class PlaidController(PlaidService plaid) : ControllerBase
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
    public async Task<IActionResult> ExchangePublicToken([FromBody] ExchangeRequestViewModel req)
    {
        await plaid.ExchangePublicTokenAsync(req.PublicToken);
        return Ok(new { status = "ok" });
    }
}

