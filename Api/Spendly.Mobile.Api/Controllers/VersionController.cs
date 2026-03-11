namespace Spendly.Mobile.Api.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Spendly.Shared.ViewModels.Settings;

[ApiController]
[Route("api/v1/version")]
public class VersionController(VersionSettings settings) : ControllerBase
{

	[HttpPost("check")]
	[AllowAnonymous]
	[Produces("application/json")]
	public IActionResult CheckVersion([FromBody] VersionCheckRequest request)
	{
		if (string.IsNullOrWhiteSpace(request.Version))
		{
			return BadRequest(new {success = false, message = "version required"});
		}

		var isThereNewVersion = IsThereANewVersion(request.Version, settings.MinSupportedVersion);

		return Ok(new
		{
			isThereNewVersion = isThereNewVersion, forceUpdate = settings.ForceUpdate && !isThereNewVersion, appleStoreUrl = settings.AppleStoreUrl, playStoreUrl = settings.PlayStoreUrl,
			requiredVersion = settings.MinSupportedVersion,
			localizedMessages = settings.LocalizedMessages
		});
	}

	private static bool IsThereANewVersion(string clientVersion, string supportedVersion)
	{
		if (string.IsNullOrWhiteSpace(clientVersion) ||
		    string.IsNullOrWhiteSpace(supportedVersion))
		{
			return false;
		}

		var clientParts = clientVersion.Split('.');
		var supportedParts = supportedVersion.Split('.');

		var clientMajor = Convert.ToInt32(clientParts[0]);
		var clientMinor = Convert.ToInt32(clientParts[1]);
		var clientPatch = Convert.ToInt32(clientParts[2]);

		var supportedMajor = Convert.ToInt32(supportedParts[0]);
		var supportedMinor = Convert.ToInt32(supportedParts[1]);
		var supportedPatch = Convert.ToInt32(supportedParts[2]);

		if (clientMajor < supportedMajor)
		{
			return true;
		}
		if (clientMajor > supportedMajor)
		{
			return false;
		}

		if (clientMinor < supportedMinor)
		{
			return true;
		}
		if (clientMinor > supportedMinor)
		{
			return false;
		}

		if (clientPatch < supportedPatch)
		{
			return true;
		}
		if (clientPatch > supportedPatch)
		{
			return false;
		}

		return false;
	}
}

public class VersionCheckRequest
{
	public string? Version { get; set; }
}
