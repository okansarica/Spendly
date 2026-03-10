// CHANGED_BY_AI: 2026-03-06 - Add banks and accounts service
namespace Spendly.Mobile.BusinessLayer.Services.Finance;

using Microsoft.AspNetCore.DataProtection;
using MongoDB.Bson;
using Shared.Entities.Banking;
using Spendly.Mobile.BusinessLayer.Services.Plaid;
using Spendly.Mobile.ViewModels.Finance;
using Spendly.Shared.Core;
using Spendly.Shared.DataLayer;
using Spendly.Shared.Entities.UserManagement;
using Spendly.Shared.Localization;
using Spendly.Shared.ViewModels;

public class BankService(
	IRepository<Bank> bankRepository,
	IRepository<Account> accountRepository,
	IRepository<BankDefinition> bankDefinitionRepository,
	IRepository<UserPlaidToken> userPlaidTokenRepository,
	IDataProtectionProvider dataProtectionProvider,
	PlaidService plaidService,
	RequestContextViewModel requestContextViewModel)
{
	public async Task<FunctionResponse<List<BankListItemViewModel>>> ListAsync(BankListRequestViewModel request)
	{
		var userId = requestContextViewModel.UserId.ToObjectId();
		var banks = (await bankRepository.ListAsync(x => x.UserId == userId)).ToList();
		var accounts = await accountRepository.ListDictionaryAsync(banks.Select(p => p.Id), p => p.BankId);

		var bankDefinitionIds = banks.Where(p => p.BankDefinitionId is not null).Select(p => p.BankDefinitionId!.Value).ToList();
		var bankDefinitions = new Dictionary<ObjectId, BankDefinition>();
		if (bankDefinitionIds.Any())
		{
			bankDefinitions = await bankDefinitionRepository.ListDictionaryAsync(bankDefinitionIds);
		}

		var response = new List<BankListItemViewModel>();

		foreach (var bank in banks.OrderBy(x => x.Name))
		{
			var item = new BankListItemViewModel
			{
				Id = bank.Id.ToString(),
				Name = bank.BankDefinitionId.HasValue ? bankDefinitions[bank.BankDefinitionId.Value].Name : bank.Name!,
				Description = bank.Description,
				BankDefinitionId = bank.BankDefinitionId?.ToString(),
				IsConnected = bank.IsConnected,
				Accounts = new List<BankAccountListItemViewModel>()
			};

			if (accounts.ContainsKey(bank.Id))
			{
				foreach (var account in accounts[bank.Id].OrderBy(p => p.Name))
				{
					item.Accounts.Add(new BankAccountListItemViewModel
					{
						Id = account.Id.ToString(),
						Name = account.Name,
						NickName = account.NickName,
						Description = account.Description,
						IsConnected = account.IsConnected,
						Mask = account.Mask
					});
				}	
			}

			response.Add(item);
		}

		response = response
			.OrderBy(p => p.Name)
			.ToList();

		return FunctionResponse.Success(response);
	}

	public async Task<FunctionResponse<List<BankDefinitionListItemViewModel>>> ListDefinitionsAsync()
	{
		var definitions = (await bankDefinitionRepository.ListAsync(x => true)).ToList();
		var response = definitions.OrderBy(x => x.Name)
			.Select(x => new BankDefinitionListItemViewModel
			{
				Id = x.Id.ToString(),
				Name = x.Name,
				LogoName = x.LogoName,
			})
			.OrderBy(p => p.Name)
			.ToList();

		return FunctionResponse.Success(response);
	}

	public async Task<FunctionResponse<BankListItemViewModel>> CreateAsync(BankUpsertRequestViewModel request)
	{
		var userId = requestContextViewModel.UserId.ToObjectId();

		if (string.IsNullOrEmpty(request.BankDefinitionId))
		{
			var existingBank = await bankRepository.GetAsync(x => x.Name == request.Name && x.UserId == userId);
			if (existingBank != null)
			{
				return FunctionResponse.Failure<BankListItemViewModel>(MessageCodes.DuplicateBank);
			}
		}
		else
		{
			var existingBank = await bankRepository.GetAsync(x => x.BankDefinitionId == request.BankDefinitionId!.ToObjectId() && x.UserId == userId);
			if (existingBank != null)
			{
				return FunctionResponse.Failure<BankListItemViewModel>(MessageCodes.DuplicateBank);
			}
		}

		var bank = new Bank
		{
			UserId = userId,
			Name = string.IsNullOrEmpty(request.BankDefinitionId) ? request.Name:null,
			Description = request.Description?.Trim(),
			BankDefinitionId = request.BankDefinitionId.ToObjectIdOrNull(),
			IsConnected = false,
		};

		await bankRepository.InsertAsync(bank);

		var bankName = bank.Name;
		if (request.BankDefinitionId is not null)
		{
			var bankDefinition = await bankDefinitionRepository.GetRequiredAsync(request.BankDefinitionId.ToObjectId());
			bankName = bankDefinition.Name;
		}

		return FunctionResponse.Success(ToBankResponse(bank, bankName!, []));
	}

	public async Task<FunctionResponse<BankListItemViewModel>> UpdateAsync(string id, BankUpsertRequestViewModel request)
	{
		var bankId = id.ToObjectId();
		var userId = requestContextViewModel.UserId.ToObjectId();
		var bank = await bankRepository.GetRequiredAsync(x => x.Id == bankId && x.UserId == userId);

		bank.Description = request.Description?.Trim();
		bank.UpdatedAt = DateTime.UtcNow;
		bank.Name = request.Name;

		await bankRepository.UpdateAsync(bank);

		var bankName = bank.Name;
		if (request.BankDefinitionId is not null)
		{
			var bankDefinition = await bankDefinitionRepository.GetRequiredAsync(request.BankDefinitionId.ToObjectId());
			bankName = bankDefinition.Name;
		}

		var accounts = await accountRepository.ListAsync(x => x.BankId == bankId);

		return FunctionResponse.Success(ToBankResponse(bank, bankName, accounts.Select(ToAccountResponse).ToList()));
	}

	public async Task<FunctionResponse> DeleteAsync(string id)
	{
		var bankId = id.ToObjectIdOrNull();
		if (bankId == null)
		{
			return FunctionResponse.Failure(MessageCodes.InvalidBankId);
		}

		var userId = requestContextViewModel.UserId.ToObjectId();
		var bank = await bankRepository.GetAsync(x => x.Id == bankId && x.UserId == userId);
		if (bank == null)
		{
			return FunctionResponse.Failure(MessageCodes.BankNotFound);
		}

		if (bank.UserPlaidTokenId.HasValue)
		{
			var userPlaidToken = await userPlaidTokenRepository.GetRequiredAsync(bank.UserPlaidTokenId.Value);

			var protector = dataProtectionProvider.CreateProtector("UserPlaidTokenProtector");
			var accessToken = protector.Unprotect(userPlaidToken.EncryptedAccessToken);

			await plaidService.RemoveItemAsync(accessToken);
			await userPlaidTokenRepository.DeleteAsync(userPlaidToken.Id);

			var accounts = await accountRepository.ListAsync(x => x.BankId == bank.Id);
			foreach (var account in accounts)
			{
				await accountRepository.DeleteAsync(account.Id);
			}

			await bankRepository.DeleteAsync(bank.Id);
			return FunctionResponse.Success();
		}

		var manualAccounts = await accountRepository.ListAsync(x => x.BankId == bank.Id);
		if (manualAccounts.Any())
		{
			return FunctionResponse.Failure(MessageCodes.BankHasAccounts);
		}

		await bankRepository.DeleteAsync(bank.Id);
		return FunctionResponse.Success();
	}

	public async Task<FunctionResponse<BankAccountListItemViewModel>> CreateAccountAsync(string bankId, BankAccountUpsertRequestViewModel request)
	{
		var bankObjectId = bankId.ToObjectIdOrNull();
		if (bankObjectId == null)
		{
			return FunctionResponse.Failure<BankAccountListItemViewModel>(MessageCodes.InvalidBankId);
		}

		var userId = requestContextViewModel.UserId.ToObjectId();
		var bank = await bankRepository.GetAsync(x => x.Id == bankObjectId && x.UserId == userId);
		if (bank == null)
		{
			return FunctionResponse.Failure<BankAccountListItemViewModel>(MessageCodes.BankNotFound);
		}

		var account = new Account
		{
			BankId = bank.Id,
			Name = request.Name!.Trim(),
			NickName = request.NickName?.Trim(),
			IsConnected = false,
		};

		await accountRepository.InsertAsync(account);

		return FunctionResponse.Success(ToAccountResponse(account));
	}

	public async Task<FunctionResponse<BankAccountListItemViewModel>> UpdateAccountAsync(string bankId, string id, BankAccountUpsertRequestViewModel request)
	{
		var bankObjectId = bankId.ToObjectIdOrNull();
		if (bankObjectId == null)
		{
			return FunctionResponse.Failure<BankAccountListItemViewModel>(MessageCodes.InvalidBankId);
		}

		var accountId = id.ToObjectIdOrNull();
		if (accountId == null)
		{
			return FunctionResponse.Failure<BankAccountListItemViewModel>(MessageCodes.InvalidAccountId);
		}

		var userId = requestContextViewModel.UserId.ToObjectId();
		var bank = await bankRepository.GetAsync(x => x.Id == bankObjectId && x.UserId == userId);
		if (bank == null)
		{
			return FunctionResponse.Failure<BankAccountListItemViewModel>(MessageCodes.BankNotFound);
		}

		var account = await accountRepository.GetAsync(x => x.Id == accountId && x.BankId == bank.Id);
		if (account == null)
		{
			return FunctionResponse.Failure<BankAccountListItemViewModel>(MessageCodes.AccountNotFound);
		}

		account.Name = request.Name!.Trim();
		account.NickName = request.NickName?.Trim();
		account.UpdatedAt = DateTime.UtcNow;

		await accountRepository.UpdateAsync(account);

		return FunctionResponse.Success(ToAccountResponse(account));
	}

	public async Task<FunctionResponse> DeleteAccountAsync(string bankId, string id)
	{
		var bankObjectId = bankId.ToObjectIdOrNull();
		if (bankObjectId == null)
		{
			return FunctionResponse.Failure(MessageCodes.InvalidBankId);
		}

		var accountId = id.ToObjectIdOrNull();
		if (accountId == null)
		{
			return FunctionResponse.Failure(MessageCodes.InvalidAccountId);
		}

		var userId = requestContextViewModel.UserId.ToObjectId();
		var bank = await bankRepository.GetAsync(x => x.Id == bankObjectId && x.UserId == userId);
		if (bank == null)
		{
			return FunctionResponse.Failure(MessageCodes.BankNotFound);
		}

		var account = await accountRepository.GetAsync(x => x.Id == accountId && x.BankId == bank.Id);
		if (account == null)
		{
			return FunctionResponse.Failure(MessageCodes.AccountNotFound);
		}

		await accountRepository.DeleteAsync(account.Id);
		return FunctionResponse.Success();
	}

	private static BankListItemViewModel ToBankResponse(Bank bank, string bankName, List<BankAccountListItemViewModel> accounts)
	{
		return new BankListItemViewModel
		{
			Id = bank.Id.ToString(),
			Name = bankName,
			Description = bank.Description,
			BankDefinitionId = bank.BankDefinitionId?.ToString(),
			IsConnected = bank.IsConnected,
			Accounts = accounts
		};
	}

	private static BankAccountListItemViewModel ToAccountResponse(Account account)
	{
		return new BankAccountListItemViewModel
		{
			Id = account.Id.ToString(),
			Name = account.Name,
			NickName = account.NickName,
			Description = account.Description,
			IsConnected = account.IsConnected,
			Mask = account.Mask,
		};
	}

	public async Task<(Bank bank, UserPlaidToken userPlaidToken)> GetBankAndUserPlaidTokenWithItemId(string itemId)
	{
		var userPlaidToken = await userPlaidTokenRepository.GetRequiredAsync(p => p.ItemId == itemId);

		var bank = await bankRepository.GetRequiredAsync(p => p.UserPlaidTokenId == userPlaidToken.Id);

		return (bank, userPlaidToken);
	}
}
