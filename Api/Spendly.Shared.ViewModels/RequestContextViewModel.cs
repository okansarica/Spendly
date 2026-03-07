// CHANGED_BY_AI: 2026-03-02 - Add timezone to request context

namespace Spendly.Shared.ViewModels;

using System.Security.Authentication;

public class RequestContextViewModel
{
	//public string SellerId { get; set; }
	private string? _userId;

	public string UserId
	{
		get
		{
			if (string.IsNullOrEmpty(_userId))
			{
				throw new AuthenticationException("Id claim not found");
			}
			return _userId;
		}
	}

	public string? TryToGetUserId()
	{
		return _userId;
	}

	public void SetUserId(string? sellerId)
	{
		_userId = sellerId;
	}

	public Guid SessionId { get; set; }
	public string Timezone { get; set; } = "Europe/London";
	public string Language { get; set; } = "en";
	//TODO review timezone
}
