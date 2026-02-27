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
}
