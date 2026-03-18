// CHANGED_BY_AI: 2026-03-10 - Add update-mode Plaid integration flow for connected bank account additions
namespace Spendly.Mobile.BusinessLayer.Services.Plaid;

using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using Shared.Core.Extensions;
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
using System.Diagnostics;
using System.Net.Http.Json;
using System.Text.Json;

//https://plaid.com/docs/api/items/#itempublic_tokenexchange

public class PlaidService(
	HttpClient httpClient,
	IDataProtectionProvider dataProtectionProvider,
	IRepository<UserPlaidToken> userPlaidTokenRepository,
	LogRepository<PlaidCommunicationLog> plaidCommunicationLogRepository,
	IRepository<Bank> bankRepository,
	IRepository<Account> accountRepository,
	PlaidSettings plaidSettings,
	RequestContextViewModel requestContextViewModel,
	ILogger<PlaidService> logger)
{

	private readonly IDataProtector _protector =
		dataProtectionProvider.CreateProtector("UserPlaidTokenProtector");

	public async Task<string> CreateLinkTokenAsync(CreateLinkTokenRequestViewModel? createLinkTokenRequestViewModel)
	{
		var mode = createLinkTokenRequestViewModel?.Mode ?? PlaidFlowMode.Create;
		var accessToken = mode == PlaidFlowMode.Update ? await GetUpdateModeAccessToken(createLinkTokenRequestViewModel) : null;

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
			redirect_uri = plaidSettings.RedirectUrl,
			access_token = accessToken,
			update = mode == PlaidFlowMode.Update ? new {account_selection_enabled = true} : null,
			webhook = "https://yourdomain.com/plaid/webhook" //TODO url
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
		if (completeIntegrationRequestViewModel.Mode == PlaidFlowMode.Update)
		{
			return await CompleteIntegrationForUpdateMode(completeIntegrationRequestViewModel);
		}

		var existingBank = await bankRepository.GetIncludingSoftDeletedAsync(p =>
			p.UserId == requestContextViewModel.UserId.ToObjectId() && p.PlaidInstitutionId == completeIntegrationRequestViewModel.Institution.Id);

		if (existingBank != null)
		{
			if (!existingBank.IsDeleted)
			{
				//Eger varolan banka tekrar eklenmeye calisirsa plaid tarafinda duplicate item olusturmamak icin exchange token yapilmaz. Kullanici update mode a yonlendirilir
				return FunctionResponse.Failure<CompleteIntegrationResponseViewModel>(MessageCodes.BankCanNotBeAddedMultipleTimes);
			}
		}

		var exchangePublicTokenResponse = await ExchangePublicTokenAsync(completeIntegrationRequestViewModel.PublicToken);
		var userPlaidToken = await SaveAccessToken(exchangePublicTokenResponse);
		var bank = await SaveBank(userPlaidToken.Id, completeIntegrationRequestViewModel.Institution, existingBank);
		var newAccountPlaidIds = await SaveAccounts(bank.Id, completeIntegrationRequestViewModel.Accounts);

		return FunctionResponse.Success(new CompleteIntegrationResponseViewModel
		{
			AccessToken = exchangePublicTokenResponse.AccessToken,
			BankId = bank.Id.ToString(),
			NewAccountPlaidIds = newAccountPlaidIds
		});
	}

	private async Task<FunctionResponse<CompleteIntegrationResponseViewModel>> CompleteIntegrationForUpdateMode(CompleteIntegrationRequestViewModel completeIntegrationRequestViewModel)
	{
		if (string.IsNullOrEmpty(completeIntegrationRequestViewModel.BankId))
		{
			return FunctionResponse.Failure<CompleteIntegrationResponseViewModel>(MessageCodes.InvalidBankId);
		}

		var bankId = completeIntegrationRequestViewModel.BankId.ToObjectId();
		var bank = await bankRepository.GetRequiredAsync(p => p.Id == bankId && p.UserId == requestContextViewModel.UserId.ToObjectId());

		if (!bank.UserPlaidTokenId.HasValue)
		{
			throw new Exception($"Connected bank has no UserPlaidTokenId. BankId: {bank.Id}");
		}

		var userPlaidToken = await userPlaidTokenRepository.GetRequiredAsync(bank.UserPlaidTokenId.Value);
		var accessToken = _protector.Unprotect(userPlaidToken.EncryptedAccessToken);
		var newAccountPlaidIds = await SaveAccounts(bank.Id, completeIntegrationRequestViewModel.Accounts);

		return FunctionResponse.Success(new CompleteIntegrationResponseViewModel
		{
			AccessToken = accessToken,
			BankId = bank.Id.ToString(),
			NewAccountPlaidIds = newAccountPlaidIds
		});
	}

	private async Task<string?> GetUpdateModeAccessToken(CreateLinkTokenRequestViewModel? createLinkTokenRequestViewModel)
	{
		if (createLinkTokenRequestViewModel is null ||
		    string.IsNullOrEmpty(createLinkTokenRequestViewModel.BankId))
		{
			throw new Exception("BankId is required for update mode link token creation.");
		}

		var bankId = createLinkTokenRequestViewModel.BankId.ToObjectId();
		var bank = await bankRepository.GetRequiredAsync(p => p.Id == bankId && p.UserId == requestContextViewModel.UserId.ToObjectId());
		if (!bank.UserPlaidTokenId.HasValue)
		{
			throw new Exception($"Connected bank has no UserPlaidTokenId. BankId: {bank.Id}");
		}

		var userPlaidToken = await userPlaidTokenRepository.GetRequiredAsync(bank.UserPlaidTokenId.Value);
		return _protector.Unprotect(userPlaidToken.EncryptedAccessToken);
	}

	private async Task<List<string>> SaveAccounts(ObjectId bankId, List<PlaidAccountViewModel> plaidAccounts)
	{
		var allExistingAccountsIncludingDeleted = await accountRepository.ListIncludingSoftDeletedAsync(p => p.BankId == bankId);

		var newAccountPlaidIds = new List<string>();
		foreach (var plaidAccount in plaidAccounts)
		{
			var existingAccount = allExistingAccountsIncludingDeleted.SingleOrDefault(p => p.PlaidAccountId == plaidAccount.Id);
			if (existingAccount != null &&
			    !existingAccount.IsDeleted)
			{
				//boyle bir account zaten var ve silinmemis 
				logger.LogInformation(
					$"Account already exists not adding one more time. existingPlaisAccountIds:{JsonSerializer.Serialize(allExistingAccountsIncludingDeleted.Select(p => new {p.Id, p.PlaidAccountId, p.IsDeleted}))}, PlaidAccountId:{plaidAccount.Id}");
				continue;
			}

			if (existingAccount == null)
			{
				//eger hesap yoksa mask + subtype eslesmesi dene
				existingAccount = allExistingAccountsIncludingDeleted.SingleOrDefault(p => p.IsDeleted && p.Mask == plaidAccount.Mask && p.Subtype == plaidAccount.Subtype);
			}

			if (existingAccount != null &&
			    existingAccount.IsDeleted)
			{
				//boyle bir account var ama silinmis
				existingAccount.IsDeleted = false;
				existingAccount.DeletedAt = null;
				existingAccount.Name = plaidAccount.Name;
				existingAccount.Mask = plaidAccount.Mask;
				existingAccount.ConnectionDateTime = DateTime.UtcNow;

				await accountRepository.UpdateAsync(existingAccount).ConfigureAwait(false);
				newAccountPlaidIds.Add(plaidAccount.Id);

				continue;
			}

			var account = new Account
			{
				BankId = bankId,
				CurrencyCode = "GBP",
				IsConnected = true,
				Name = plaidAccount.Name,
				PlaidAccountId = plaidAccount.Id,
				Mask = plaidAccount.Mask,
				ConnectionDateTime = DateTime.UtcNow,
				Subtype = plaidAccount.Subtype,
			};
			await accountRepository.InsertAsync(account).ConfigureAwait(false);
			newAccountPlaidIds.Add(plaidAccount.Id);
		}

		foreach (var existingAccount in allExistingAccountsIncludingDeleted)
		{
			var plaidAccount = plaidAccounts.SingleOrDefault(p => p.Id == existingAccount.PlaidAccountId);
			if (plaidAccount is null &&
			    !existingAccount.IsDeleted)
			{
				await accountRepository.DeleteAsync(existingAccount.Id);
			}
		}

		return newAccountPlaidIds;
	}

	private async Task<Bank> SaveBank(ObjectId userPlaidTokenId, PlaidInstitutionViewModel institude, Bank? softDeletedBank = null)
	{
		if (softDeletedBank != null)
		{
			softDeletedBank.IsDeleted = false;
			softDeletedBank.DeletedAt = null;
			softDeletedBank.IsConnected = true;
			softDeletedBank.Name = institude.Name;
			softDeletedBank.UserPlaidTokenId = userPlaidTokenId;
			softDeletedBank.ConnectionDateTime = DateTime.UtcNow;
			await bankRepository.UpdateAsync(softDeletedBank);
			return softDeletedBank;
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


	public async Task RemoveItemAsync(string accessToken)
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
			Type = PlaidCommunicationLogType.RemoveItemRequest
		});

		var response = await httpClient.PostAsJsonAsync(
			plaidSettings.BaseUrl + "/item/remove",
			request);

		var responseContent = await response.Content.ReadAsStringAsync();

		await plaidCommunicationLogRepository.InsertAsync(new PlaidCommunicationLog
		{
			Content = responseContent,
			Type = PlaidCommunicationLogType.RemoveItemResponse
		});

		if (!response.IsSuccessStatusCode)
		{
			logger.LogError(
				"Plaid item/remove failed for user {UserId} status {Status}. Response: {Response}",
				requestContextViewModel.UserId,
				response.StatusCode,
				responseContent);

			throw new Exception($"Plaid item/remove failed. Response: {responseContent}");
		}
	}
}
