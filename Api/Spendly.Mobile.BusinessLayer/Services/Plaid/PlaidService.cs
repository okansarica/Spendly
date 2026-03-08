namespace Spendly.Mobile.BusinessLayer.Services.Plaid;

using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using Shared.Localization;
using Spendly.Mobile.BusinessLayer.Constants;
using Spendly.Mobile.ViewModels.Plaid;
using Spendly.Shared.Core;
using Spendly.Shared.DataLayer;
using Spendly.Shared.Entities.Banking;
using Spendly.Shared.Entities.UserManagement;
using Spendly.Shared.ViewModels;
using Spendly.Shared.ViewModels.Plaid;
using Spendly.Shared.ViewModels.Settings;
using System.Net.Http.Json;
using System.Text.Json;

//https://plaid.com/docs/api/items/#itempublic_tokenexchange

public class PlaidService(
	HttpClient httpClient,
	IDataProtectionProvider dataProtectionProvider,
	IRepository<UserPlaidToken> userPlaidTokenRepository,
	IRepository<PlaidCommunicationLog> plaidCommunicationLogRepository,
	IRepository<Bank> bankRepository,
	IRepository<Account> accountRepository,
	PlaidSettings plaidSettings,
	RequestContextViewModel requestContextViewModel,
	ILogger<PlaidService> logger)
{

	private readonly IDataProtector _protector =
		dataProtectionProvider.CreateProtector("UserPlaidTokenProtector");

	public async Task<string> CreateLinkTokenAsync()
	{
		//TODO make it strongly typed
		var request = new
		{
			client_id = plaidSettings.ClientId,
			secret = plaidSettings.Secret,
			client_name = Constants.Application.ApplicationNAme,
			country_codes = new[] {"GB"},
			language = requestContextViewModel.Language,
			user = new {client_user_id = requestContextViewModel.UserId},
			products = new[] {"transactions"},
			redirect_uri = plaidSettings.RedirectUrl
		};

		var requestJson = JsonSerializer.Serialize(request);

		await plaidCommunicationLogRepository.InsertAsync(new PlaidCommunicationLog
		{
			Content = requestJson,
			Type = PlaidCommunicationLogType.CreateTLinkTokenRequest
		});

		var response = await httpClient.PostAsJsonAsync(
			plaidSettings.BaseUrl + "/link/token/create",
			request);

		var responseContent = await response.Content.ReadAsStringAsync();

		await plaidCommunicationLogRepository.InsertAsync(new PlaidCommunicationLog
		{
			Content = responseContent,
			Type = PlaidCommunicationLogType.CreateTLinkTokenResponse
		});

		if (!response.IsSuccessStatusCode)
		{
			logger.LogWarning(
				"Plaid link token create failed for user {UserId} with status {Status}. Response: {Response}",
				requestContextViewModel.UserId,
				response.StatusCode,
				responseContent);

			throw new Exception($"Plaid link token creation failed. Response: {responseContent}");
		}

		//TODO make strongly typed
		var body = JsonSerializer.Deserialize<JsonDocument>(responseContent);

		var linkToken = body!
			.RootElement
			.GetProperty("link_token")
			.GetString()!;

		return linkToken;
	}

	public async Task<FunctionResponse<CompleteIntegrationResponseViewModel>> CompleteIntegration(CompleteIntegrationRequestViewModel completeIntegrationRequestViewModel)
	{
		var existingBank =  await bankRepository.GetAsync(p => p.UserId == requestContextViewModel.UserId.ToObjectId() &&p.PlaidInstitutionId == completeIntegrationRequestViewModel.Institution.Id);

		if (existingBank != null)
		{
			//Eger varolan banka tekrar eklenmeye calisirsa plaid tarafinda duplicate item olusturmamak icin exchange token yapilmaz. Kullanici update mode a yonlendirilir
			return FunctionResponse.Failure<CompleteIntegrationResponseViewModel>(MessageCodes.BankCanNotBeAddedMultipleTimes);
		}
		
		var exchangePublicTokenResponse = await ExchangePublicTokenAsync(completeIntegrationRequestViewModel.PublicToken);
		var userPlaidToken = await SaveAccessToken(exchangePublicTokenResponse);
		var bank = await SaveBank(userPlaidToken.Id, completeIntegrationRequestViewModel.Institution);
		var newAccountIds = await SaveAccounts(bank.Id, completeIntegrationRequestViewModel.Accounts);

		return FunctionResponse.Success(new CompleteIntegrationResponseViewModel
		{
			AccessToken = exchangePublicTokenResponse.AccessToken,
			BankId = bank.Id.ToString(),
			//NewAccountPlaidIds = newAccountIds.Select(p=>p.ToString()).ToList()
		});
	}
	private async Task<List<ObjectId>> SaveAccounts(ObjectId bankId, List<PlaidAccountViewModel> plaidAccounts)
	{
		var newAccountIds = new List<ObjectId>();
		foreach (var plaidAccount in plaidAccounts)
		{
			var account = new Account
			{
				BankId = bankId,
				CurrencyCode = "GBP", //TODO hard coded for now, requires more changes in the ui for future
				IsConnected = true,
				Name = plaidAccount.Name??"N/A", //TODO check
				PlaidAccountId = plaidAccount.Id,
				Mask = plaidAccount.Mask,
				ConnectionDateTime = DateTime.UtcNow,
			};
			await accountRepository.InsertAsync(account).ConfigureAwait(false);
			newAccountIds.Add(account.Id);
		}
		return newAccountIds;
	}
	private async Task<Bank> SaveBank(ObjectId userPlaidTokenId, PlaidInstitutionViewModel institude)
	{
		var exisingBank = await bankRepository.GetAsync(p => p.PlaidInstitutionId == institude.Id && p.UserId == requestContextViewModel.UserId.ToObjectId());
		if (exisingBank != null)
		{
			return exisingBank;
		}

		var bank = new Bank
		{
			IsConnected = true,
			Name = institude.Name,
			PlaidInstitutionId = institude.Id,
			UserId = requestContextViewModel.UserId.ToObjectId(),
			UserPlaidTokenId = userPlaidTokenId,
			ConnectionDateTime = DateTime.UtcNow,
		};
		await bankRepository.InsertAsync(bank);
		return bank;
	}


	private async Task<UserPlaidToken> SaveAccessToken((string accessToken, string itemId, DateTime? expirationDateTime) exchangePublicTokenResponse)
	{
		var protectedToken = _protector.Protect(exchangePublicTokenResponse.accessToken);

		var existingUserPlaidToken =
			await userPlaidTokenRepository.GetAsync(x => x.UserId == requestContextViewModel.UserId.ToObjectId());

		if (existingUserPlaidToken != null)
		{
			existingUserPlaidToken.EncryptedAccessToken = protectedToken;
			existingUserPlaidToken.ItemId = exchangePublicTokenResponse.itemId;
			existingUserPlaidToken.ExpirationDateTime = exchangePublicTokenResponse.expirationDateTime;

			await userPlaidTokenRepository.UpdateAsync(existingUserPlaidToken);

			return existingUserPlaidToken;
		}
		else
		{
			var userPlaidToken = new UserPlaidToken
			{
				UserId = requestContextViewModel.UserId.ToObjectId(),
				EncryptedAccessToken = protectedToken,
				ItemId = exchangePublicTokenResponse.itemId,
				ExpirationDateTime = exchangePublicTokenResponse.expirationDateTime
			};

			await userPlaidTokenRepository.InsertAsync(userPlaidToken);
			return userPlaidToken;
		}
	}

	private async Task<(string AccessToken, string ItemId, DateTime? ExpirationDateTime)> ExchangePublicTokenAsync(string publicToken)
	{
		var request = new
		{
			client_id = plaidSettings.ClientId,
			secret = plaidSettings.Secret,
			public_token = publicToken
		};

		var requestJson = JsonSerializer.Serialize(request);

		await plaidCommunicationLogRepository.InsertAsync(new PlaidCommunicationLog
		{
			Content = requestJson,
			Type = PlaidCommunicationLogType.ExchangePublicTokenRequest
		});

		var response = await httpClient.PostAsJsonAsync(
			plaidSettings.BaseUrl + "/item/public_token/exchange",
			request);

		var responseContent = await response.Content.ReadAsStringAsync();

		await plaidCommunicationLogRepository.InsertAsync(new PlaidCommunicationLog
		{
			Content = responseContent,
			Type = PlaidCommunicationLogType.ExchangePublicTokenResponse
		});

		if (!response.IsSuccessStatusCode)
		{
			logger.LogError(
				"Plaid public_token exchange failed for user {UserId} status {Status}. Response: {Response}",
				requestContextViewModel.UserId,
				response.StatusCode,
				responseContent);

			throw new Exception($"Plaid token exchange failed. Response: {responseContent}");
		}

		var body = JsonSerializer.Deserialize<JsonDocument>(responseContent);

		var accessToken = body!
			.RootElement
			.GetProperty("access_token")
			.GetString()!;

		var itemId = body
			.RootElement
			.GetProperty("item_id")
			.GetString()!;

		DateTime? expirationDateTime = null;

		if (body.RootElement.TryGetProperty("expiration", out var expProp))
		{
			expirationDateTime = expProp.GetDateTime();
		}
		return (accessToken, itemId, expirationDateTime);
	}

	private async Task<PlaidInstitutionViewModel> GetInstitutionAsync(string accessToken)
	{
		var itemRequest = new
		{
			client_id = plaidSettings.ClientId,
			secret = plaidSettings.Secret,
			access_token = accessToken
		};

		var itemRequestJson = JsonSerializer.Serialize(itemRequest);

		await plaidCommunicationLogRepository.InsertAsync(new PlaidCommunicationLog
		{
			Content = itemRequestJson,
			Type = PlaidCommunicationLogType.GetItemRequest
		});

		var itemResponse = await httpClient.PostAsJsonAsync(
			plaidSettings.BaseUrl + "/item/get",
			itemRequest);

		var itemResponseContent = await itemResponse.Content.ReadAsStringAsync();

		await plaidCommunicationLogRepository.InsertAsync(new PlaidCommunicationLog
		{
			Content = itemResponseContent,
			Type = PlaidCommunicationLogType.GetItemResponse
		});

		if (!itemResponse.IsSuccessStatusCode)
		{
			logger.LogError(
				"Plaid item/get failed for user {UserId} status {Status}. Response: {Response}",
				requestContextViewModel.UserId,
				itemResponse.StatusCode,
				itemResponseContent);

			throw new Exception($"Plaid item/get failed. Response: {itemResponseContent}");
		}

		var itemBody = JsonSerializer.Deserialize<JsonDocument>(itemResponseContent);
		var institutionId = itemBody!
			.RootElement
			.GetProperty("item")
			.GetProperty("institution_id")
			.GetString()!;

		var institutionRequest = new
		{
			client_id = plaidSettings.ClientId,
			secret = plaidSettings.Secret,
			institution_id = institutionId,
			country_codes = new[] {"GB"}
		};

		var institutionRequestJson = JsonSerializer.Serialize(institutionRequest);

		await plaidCommunicationLogRepository.InsertAsync(new PlaidCommunicationLog
		{
			Content = institutionRequestJson,
			Type = PlaidCommunicationLogType.GetInstitutionRequest
		});

		var institutionResponse = await httpClient.PostAsJsonAsync(
			plaidSettings.BaseUrl + "/institutions/get_by_id",
			institutionRequest);

		var institutionResponseContent = await institutionResponse.Content.ReadAsStringAsync();

		await plaidCommunicationLogRepository.InsertAsync(new PlaidCommunicationLog
		{
			Content = institutionResponseContent,
			Type = PlaidCommunicationLogType.GetInstitutionResponse
		});

		if (!institutionResponse.IsSuccessStatusCode)
		{
			logger.LogError(
				"Plaid institutions/get_by_id failed for user {UserId} status {Status}. Response: {Response}",
				requestContextViewModel.UserId,
				institutionResponse.StatusCode,
				institutionResponseContent);

			throw new Exception($"Plaid institutions/get_by_id failed. Response: {institutionResponseContent}");
		}

		var institutionBody = JsonSerializer.Deserialize<JsonDocument>(institutionResponseContent);
		var institution = institutionBody!.RootElement.GetProperty("institution");

		return new PlaidInstitutionViewModel()
		{
			Id = institutionId,
			Name = institution.GetProperty("name").GetString()!
		};
	}

	private async Task<List<PlaidAccountViewModel>> GetAccountsAsync(string accessToken)
	{
		var request = new
		{
			client_id = plaidSettings.ClientId,
			secret = plaidSettings.Secret,
			access_token = accessToken
		};

		var requestJson = JsonSerializer.Serialize(request);

		await plaidCommunicationLogRepository.InsertAsync(new PlaidCommunicationLog
		{
			Content = requestJson,
			Type = PlaidCommunicationLogType.GetAccountsRequest
		});

		var response = await httpClient.PostAsJsonAsync(
			plaidSettings.BaseUrl + "/accounts/get",
			request);

		var responseContent = await response.Content.ReadAsStringAsync();

		await plaidCommunicationLogRepository.InsertAsync(new PlaidCommunicationLog
		{
			Content = responseContent,
			Type = PlaidCommunicationLogType.GetAccountsResponse
		});

		if (!response.IsSuccessStatusCode)
		{
			logger.LogError(
				"Plaid accounts/get failed for user {UserId} status {Status}. Response: {Response}",
				requestContextViewModel.UserId,
				response.StatusCode,
				responseContent);

			throw new Exception($"Plaid accounts/get failed. Response: {responseContent}");
		}

		var body = JsonSerializer.Deserialize<JsonDocument>(responseContent);
		var accounts = body!.RootElement.GetProperty("accounts");

		var result = new List<PlaidAccountViewModel>();

		foreach (var account in accounts.EnumerateArray())
		{
			var accountId = account.GetProperty("account_id").GetString()!;
			var name = account.GetProperty("name").GetString()!;
			var officialName = account.TryGetProperty("official_name", out var officialNameProp) ? officialNameProp.GetString() : null;
			var type = account.GetProperty("type").GetString()!;
			var subtype = account.TryGetProperty("subtype", out var subtypeProp) ? subtypeProp.GetString() : null;

			var mask = account.TryGetProperty("mask", out var maskProp) ? maskProp.GetString() : null;

			result.Add(new PlaidAccountViewModel
			{
				Id = accountId,
				Name = officialName ?? name,
				Type = type,
				Subtype = subtype,
				Mask = mask
			});
		}

		return result;
	}

	public async Task<List<PlaidTransactionViewModel>> GetTransactionsAsync(string accessToken,
		DateTime startDate,
		DateTime endDate,
		string userId)
	{
		var request = new
		{
			client_id = plaidSettings.ClientId,
			secret = plaidSettings.Secret,
			access_token = accessToken,
			start_date = startDate.ToString("yyyy-MM-dd"),
			end_date = endDate.ToString("yyyy-MM-dd"),
			options = new
			{
				count = 500, //TODO paging yonetilecek ilk istek icin
				offset = 0
			}
		};

		var requestJson = JsonSerializer.Serialize(request);

		await plaidCommunicationLogRepository.InsertAsync(new PlaidCommunicationLog
		{
			Content = requestJson,
			Type = PlaidCommunicationLogType.GetTransactionsRequest
		});

		var response = await httpClient.PostAsJsonAsync(
			plaidSettings.BaseUrl + "/transactions/get",
			request);

		var responseContent = await response.Content.ReadAsStringAsync();


		await plaidCommunicationLogRepository.InsertAsync(new PlaidCommunicationLog
		{
			Content = responseContent, //TODO bunu kaydetme cok buyuk olur gerek yok
			Type = PlaidCommunicationLogType.GetTransactionsResponse
		});

		if (!response.IsSuccessStatusCode)
		{
			logger.LogError(
				"Plaid transactions/get failed for user {UserId} status {Status}. Response: {Response}",
				userId,
				response.StatusCode,
				responseContent);

			throw new Exception($"Plaid transactions/get failed. Response: {responseContent}");
		}

		var body = JsonSerializer.Deserialize<PlaidTransactionsGetResponseViewModel>(responseContent);
		var transactions = body?.Transactions ?? [];
		var totalTransactions = body?.TotalTransactions ?? transactions.Count;

		logger.LogInformation("Retrieved {Count} transactions out of {Total} for user {UserId}",
			transactions.Count,
			totalTransactions,
			userId);

		var result = new List<PlaidTransactionViewModel>();

		foreach (var transaction in transactions)
		{
			transaction.Name ??= string.Empty;
			transaction.MerchantName = string.IsNullOrWhiteSpace(transaction.MerchantName) ? transaction.Name : transaction.MerchantName;
			result.Add(transaction);
		}

		return result;
	}
}
