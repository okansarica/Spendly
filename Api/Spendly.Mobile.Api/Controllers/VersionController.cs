namespace Spendly.Mobile.Api.Controllers;

using Microsoft.AspNetCore.Mvc;
using Spendly.Shared.ViewModels.Settings;

[ApiController]
[Route("api/v1/version")]
public class VersionController(VersionSettings settings) : ControllerBase
{

	[HttpPost("check")]
	[Produces("application/json")]
	public IActionResult CheckVersion([FromBody] VersionCheckRequest request)
	{
		if (string.IsNullOrWhiteSpace(request.Version))
		{
			return BadRequest(new { success = false, message = "version required" });
		}

		var minSupported = settings.MinSupportedVersion ?? string.Empty;

		bool isSatisfied = VersionMatches(request.Version, minSupported);

		return Ok(new { satisfy = isSatisfied, forceUpdate = settings.ForceUpdate && !isSatisfied });
	}

	private static bool VersionMatches(string clientVersion, string supportedVersion)
	{
		if (string.IsNullOrWhiteSpace(supportedVersion)) return true;

		var cvParts = clientVersion.Split('.');
		var svParts = supportedVersion.Split('.');

		var len = System.Math.Max(cvParts.Length, svParts.Length);

		for (int i = 0; i < len; i++)
		{
			var cv = 0;
			var sv = 0;
			int.TryParse(i < cvParts.Length ? cvParts[i] : "0", out cv);
			int.TryParse(i < svParts.Length ? svParts[i] : "0", out sv);

			if (cv < sv)
			{
				return false;
			}
			if (cv > sv)
			{
				return true;
			}
		}

		return true;
	}
}

public class VersionCheckRequest { public string? Version { get; set; } }
