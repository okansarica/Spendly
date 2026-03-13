// CHANGED_BY_AI: 2026-03-10 - Scope Plaid transaction ingestion to new account ids for update mode
namespace Spendly.Shared.BusinessLayer;

using Core.Extensions;
using DataLayer;
using Entities.Banking;
using Entities.Reporting;
using Entities.TransactionManagement;
using Entities.UserManagement;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using System.Net.Http.Json;
using System.Text.Json;
using ViewModels.Plaid;
using ViewModels.Settings;
using Notification;

public class SharedPlaidService(
	PlaidSettings plaidSettings,
	HttpClient httpClient,
	ILogger<SharedPlaidService> logger,
	IRepository<Account> accountRepository,
	IRepository<Merchant> merchantRepository,
	IRepository<UserMerchant> userMerchantRepository,
	IRepository<PlaidCommunicationLog> plaidCommunicationLogRepository,
	IRepository<NormalizedTransaction> normalizedTransactionRepository,
	IRepository<UserCategory> userCategoryRepository,
	IRepository<RawTransaction> rawTransactionRepository,
	IRepository<DailyUserExpense> dailyUserExpenseRepository,
	IRepository<DailyMerchantExpense> dailyMerchantExpenseRepository,
	IRepository<DailyAccountExpense> dailyAccountExpenseRepository,
	IRepository<DailyCategoryExpense> dailyCategoryExpenseRepository,
	IRepository<DailyCategoryAccountExpense> dailyCategoryAccountExpenseRepository,
	IRepository<Category> categoryRepository,
	IRepository<MonthlyUserExpense> monthlyUserExpenseRepository,
	IRepository<MonthlyAccountExpense> monthlyAccountExpenseRepository,
	IRepository<MonthlyCategoryAccountExpense> monthlyCategoryAccountExpenseRepository,
	IRepository<CategoryMonthlyExpense> categoryMonthlyExpenseRepository,
	IRepository<PredefinedMerchant> predefinedMerchantRepository,
	IRepository<AccountNormalizationState> accountNormalizationStateRepository,
	IRepository<MerchantMonthlyExpense> merchantMonthlyExpenseRepository,
	IRepository<FirebaseToken> firebaseTokenRepository,
	FirebaseNotificationService firebaseNotificationService)
{
	private readonly string[] _categoriesToIgnoreMerchantGeneration =
	{
		"INCOME",
		"LOAN_DISBURSEMENTS",
		"LOAN_PAYMENTS",
		"TRANSFER_IN",
		"TRANSFER_OUT",
		"BANK_FEES"
	};

	//TODO burasi sadece ilk hesap banka ve hesap(lart) ekleme flowunda gecerli, var olan bankaya hesap ekleme durumu degerlendirilecek
	public async Task TransferTransactionsFromPlaidAsync(DateOnly startDate,
		DateOnly endDate,
		PlaidDataProcessingBackgroundServiceRequestViewModel request)
	{
		//TODO eger tek gunlukse direk kaydet 

		var allTransactionResponses = await GetAllTransactionsAsync(request.AccessToken,
			startDate,
			endDate,
			request.UserId.ToObjectId(),
			request.NewAccountPlaidIds);

		if (!allTransactionResponses.Any())
		{
			//Productlar hazir olmayabiliyor, o durumda webhook bekleniyor, islem tekrar baslatiliyor
			return;
		}

		//TODO kayit sirasinda da idempotency check gerkir
		var rawTransactions = await SaveAllTransactionResponses(allTransactionResponses,
			startDate,
			endDate,
			request.UserId.ToObjectId(),
			request.NewAccountPlaidIds);

		//normalize et
		var normalizedTransactions = await NormalizeTransactions(rawTransactions, request.BankId.ToObjectId(), request.UserId.ToObjectId(),endDate);

		//rapor tablolarini doldur
		await CreateReportingData(normalizedTransactions, request.UserId.ToObjectId());

		var firebaseToken = await firebaseTokenRepository.GetAsync(p => p.UserId == request.UserId.ToObjectId());
		if (firebaseToken != null)
		{
			await firebaseNotificationService.SendTransactionProcessingCompleteAsync(firebaseToken.Token);
		}
	}
	private async Task CreateReportingData(List<NormalizedTransaction> normalizedTransactions, ObjectId userId)
	{
		if (!normalizedTransactions.Any())
		{
			return;
		}

		var accountIds = normalizedTransactions.Select(p => p.AccountId).Distinct().ToList();
		var accounts = await accountRepository.ListAsync(p => accountIds.Contains(p.Id));
		var accountIdToBankIdMap = accounts.ToDictionary(p => p.Id, p => p.BankId);

		await CreateDailyAccountExpenses(normalizedTransactions, userId, accountIdToBankIdMap).ConfigureAwait(false);
		await CreateDailyCategoryAccountExpenses(normalizedTransactions, userId, accountIdToBankIdMap).ConfigureAwait(false);
		await CreateDailyCategoryExpenses(normalizedTransactions, userId).ConfigureAwait(false);
		await CreateDailyMerchantExpenses(normalizedTransactions, userId).ConfigureAwait(false);
		await CreateDailyUserExpenses(normalizedTransactions, userId).ConfigureAwait(false);
		
		await CreateMonthlyAccountExpenses(normalizedTransactions, userId).ConfigureAwait(false);
		await CreateMonthlyCategoryAccountExpenses(normalizedTransactions, userId, accountIdToBankIdMap).ConfigureAwait(false);
		await CreateMonthlyCategoryExpenses(normalizedTransactions, userId).ConfigureAwait(false);
		await CreateMonthlyMerchantExpenses(normalizedTransactions, userId).ConfigureAwait(false);
		await CreateMonthlyUserExpenses(normalizedTransactions, userId).ConfigureAwait(false);
	}
	
	private async Task CreateDailyAccountExpenses(List<NormalizedTransaction> normalizedTransactions, ObjectId userId, Dictionary<ObjectId, ObjectId> accountIdToBankIdMap)
	{
		var groupedData = normalizedTransactions
			.GroupBy(p => new {p.AccountId, Date = p.DateTime.Date})
			.Select(g => new DailyAccountExpense
			{
				UserId = userId,
				Date = g.Key.Date.ToDateOnly(),
				BankId = accountIdToBankIdMap[g.Key.AccountId],
				AccountId = g.Key.AccountId,
				TotalAmount = g.Sum(t => t.Amount),
				TotalCount = g.Count()
			})
			.ToList();

		if (groupedData.Any())
		{
			await dailyAccountExpenseRepository.InsertManyAsync(groupedData);
		}
	}

	private async Task CreateDailyCategoryAccountExpenses(List<NormalizedTransaction> normalizedTransactions, ObjectId userId, Dictionary<ObjectId, ObjectId> accountIdToBankIdMap)
	{
		var groupedData = normalizedTransactions
			.GroupBy(p => new {p.UserCategoryId, p.AccountId, Date = p.DateTime.Date})
			.Select(g => new DailyCategoryAccountExpense
			{
				UserId = userId,
				Date = g.Key.Date.ToDateOnly(),
				UserCategoryId = g.Key.UserCategoryId,
				AccountId = g.Key.AccountId,
				BankId = accountIdToBankIdMap[g.Key.AccountId],
				TotalAmount = g.Sum(t => t.Amount),
				TotalCount = g.Count()
			})
			.ToList();

		if (groupedData.Any())
		{
			await dailyCategoryAccountExpenseRepository.InsertManyAsync(groupedData);
		}
	}
	
	private async Task CreateDailyCategoryExpenses(List<NormalizedTransaction> normalizedTransactions, ObjectId userId)
	{
		var groupedData = normalizedTransactions
			.GroupBy(p => new {p.UserCategoryId, Date = p.DateTime.Date})
			.Select(g => new DailyCategoryExpense
			{
				UserId = userId,
				CategoryId = g.Key.UserCategoryId,
				Date = g.Key.Date.ToDateOnly(),
				TotalAmount = g.Sum(t => t.Amount),
				TotalCount = g.Count()
			})
			.ToList();

		if (groupedData.Any())
		{
			await dailyCategoryExpenseRepository.InsertManyAsync(groupedData);
		}
	}
	
	private async Task CreateDailyMerchantExpenses(List<NormalizedTransaction> normalizedTransactions, ObjectId userId)
	{
		var groupedData = normalizedTransactions
			.GroupBy(p => new {p.MerchantId, Date = p.DateTime.Date})
			.Select(g => new DailyMerchantExpense
			{
				UserId = userId,
				MerchantId = g.Key.MerchantId,
				Date = g.Key.Date.ToDateOnly(),
				TotalAmount = g.Sum(t => t.Amount),
				TotalCount = g.Count()
			})
			.ToList();

		if (groupedData.Any())
		{
			await dailyMerchantExpenseRepository.InsertManyAsync(groupedData);
		}
	}
	
	private async Task CreateDailyUserExpenses(List<NormalizedTransaction> normalizedTransactions, ObjectId userId)
	{
		var groupedData = normalizedTransactions
			.GroupBy(p => p.DateTime.Date)
			.Select(g => new DailyUserExpense
			{
				UserId = userId,
				Date = g.Key.ToDateOnly(),
				TotalAmount = g.Sum(t => t.Amount),
				TotalCount = g.Count()
			})
			.ToList();

		if (groupedData.Any())
		{
			await dailyUserExpenseRepository.InsertManyAsync(groupedData);
		}
	}
	
	private async Task CreateMonthlyAccountExpenses(List<NormalizedTransaction> normalizedTransactions, ObjectId userId)
	{
		var groupedData = normalizedTransactions
			.GroupBy(p => new {p.DateTime.Year, p.DateTime.Month, p.AccountId})
			.Select(g => new MonthlyAccountExpense
			{
				AccountId = g.Key.AccountId,
				UserId = userId,
				Year = g.Key.Year,
				Month = g.Key.Month,
				TotalAmount = g.Sum(t => t.Amount),
				TotalCount = g.Count()
			})
			.ToList();

		if (groupedData.Any())
		{
			await monthlyAccountExpenseRepository.InsertManyAsync(groupedData);
		}
	}
	
	private async Task CreateMonthlyCategoryAccountExpenses(List<NormalizedTransaction> normalizedTransactions, ObjectId userId, Dictionary<ObjectId, ObjectId> accountIdToBankIdMap)
	{
		var groupedData = normalizedTransactions
			.GroupBy(p => new {p.DateTime.Year, p.DateTime.Month, p.AccountId, p.UserCategoryId})
			.Select(g => new MonthlyCategoryAccountExpense
			{
				BankId = accountIdToBankIdMap[g.Key.AccountId],
				AccountId = g.Key.AccountId,
				UserCategoryId =  g.Key.UserCategoryId,
				UserId = userId,
				Year = g.Key.Year,
				Month = g.Key.Month,
				TotalAmount = g.Sum(t => t.Amount),
				TotalCount = g.Count()
			})
			.ToList();

		if (groupedData.Any())
		{
			await monthlyCategoryAccountExpenseRepository.InsertManyAsync(groupedData);
		}
	}
	
	private async Task CreateMonthlyCategoryExpenses(List<NormalizedTransaction> normalizedTransactions, ObjectId userId)
	{
		var groupedData = normalizedTransactions
			.GroupBy(p => new {p.UserCategoryId, p.DateTime.Year, p.DateTime.Month})
			.Select(g => new CategoryMonthlyExpense
			{
				UserId = userId,
				UserCategoryId = g.Key.UserCategoryId,
				Year = g.Key.Year,
				Month = g.Key.Month,
				TotalCount = g.Count(),
				TotalAmount = g.Sum(t => t.Amount)
			})
			.ToList();

		if (groupedData.Any())
		{
			await categoryMonthlyExpenseRepository.InsertManyAsync(groupedData);
		}
	}
	
	private async Task CreateMonthlyMerchantExpenses(List<NormalizedTransaction> normalizedTransactions, ObjectId userId)
	{
		var groupedData = normalizedTransactions
			.GroupBy(p => new {p.MerchantId, p.DateTime.Year, p.DateTime.Month})
			.Select(g => new MerchantMonthlyExpense
			{
				UserId = userId,
				MerchantId = g.Key.MerchantId,
				Year = g.Key.Year,
				Month = g.Key.Month,
				TotalCount = g.Count(),
				TotalAmount = g.Sum(t => t.Amount)
			})
			.ToList();

		if (groupedData.Any())
		{
			await merchantMonthlyExpenseRepository.InsertManyAsync(groupedData);
		}
	}

	private async Task CreateMonthlyUserExpenses(List<NormalizedTransaction> normalizedTransactions, ObjectId userId)
	{
		var groupedData = normalizedTransactions
			.GroupBy(p => new {p.DateTime.Year, p.DateTime.Month})
			.Select(g => new MonthlyUserExpense
			{
				UserId = userId,
				Year = g.Key.Year,
				Month = g.Key.Month,
				TotalAmount = g.Sum(t => t.Amount),
				TotalCount = g.Count()
			})
			.ToList();

		if (groupedData.Any())
		{
			await monthlyUserExpenseRepository.InsertManyAsync(groupedData);
		}
	}

	private async Task<List<NormalizedTransaction>> NormalizeTransactions(List<RawTransaction> rawTransactions, ObjectId bankId, ObjectId userId, DateOnly date)
	{
		var allAccounts = await accountRepository.ListAsync(p => p.BankId == bankId);
		var allMerchants = await merchantRepository.ListAsync(filter: null);
		var otherMerchant = allMerchants.Single(p => p.IsOther);
		var allPredefinedMerchants = await predefinedMerchantRepository.ListAsync(filter: null);
		var otherUserMerchant = await userMerchantRepository.GetRequiredAsync(p => p.UserId == userId && p.IsOther == true);
		var otherCategory = await categoryRepository.GetRequiredAsync(p => p.IsOther);
		var userCategories = await userCategoryRepository.ListAsync(p => p.UserId == userId);
		var otherUserCategory = userCategories.Single(p => p.IsOther);
		var userMerchants = await userMerchantRepository.ListAsync(p => p.UserId == userId);

		var context = new NormalizationContext(
			userId,
			allMerchants,
			allPredefinedMerchants,
			otherMerchant,
			otherUserMerchant,
			otherUserCategory,
			otherCategory,
			userCategories,
			userMerchants);

		var accountNormalizationStates = await accountNormalizationStateRepository.ListSingleDictionaryAsync(allAccounts.Select(p => p.Id), p=>p.AccountId);

		var normalizedTransactions = new List<NormalizedTransaction>();

		foreach (var rawTransaction in rawTransactions)
		{
			ValidateRawTransaction(rawTransaction);

			var plaidAccount = rawTransaction.PlaidTransactionsGetResponse.Account!;
			var account = allAccounts.Single(p => p.PlaidAccountId == plaidAccount.AccountId);

			foreach (var plaidTransaction in rawTransaction.PlaidTransactionsGetResponse.Transactions)
			{
				if (ShouldSkip(plaidTransaction, accountNormalizationStates.TryGetValue(account.Id, out var accountNormalizationState)? accountNormalizationState:null))
				{
					continue;
				}

				ValidatePlaidTransaction(rawTransaction, plaidTransaction);

				var (merchantId, userMerchantId, userCategoryId) = await ResolveMerchantAndCategoryAsync(plaidTransaction, context);

				normalizedTransactions.Add(new NormalizedTransaction
				{
					AccountId = account.Id,
					Amount = plaidTransaction.Amount,
					DateTime = plaidTransaction.DateTime ?? plaidTransaction.Date.ToDateTime(TimeOnly.MinValue),
					MerchantId = merchantId,
					UserMerchantId = userMerchantId,
					PlaidTransactionId = plaidTransaction.TransactionId,
					RawTransactionId = rawTransaction.Id,
					UserId = userId,
					UserCategoryId = userCategoryId,
					TransactionName = plaidTransaction.Name,
				});
			}
		}

		if (normalizedTransactions.Any())
		{
			await normalizedTransactionRepository.InsertManyAsync(normalizedTransactions);
			foreach (var account in allAccounts)
			{
				if(accountNormalizationStates.TryGetValue(account.Id, out var accountNormalizationState))
				{
					accountNormalizationState.Date = date;
					await accountNormalizationStateRepository.UpdateAsync(accountNormalizationState);
				}
				else
				{
					accountNormalizationState = new AccountNormalizationState
					{
						AccountId = account.Id,
						Date = date
					};
					await accountNormalizationStateRepository.InsertAsync(accountNormalizationState);
				}
			}
		}
//TODO 
		return normalizedTransactions;
	}

	private async Task<(ObjectId merchantId, ObjectId userMerchantId, ObjectId userCategoryId)> ResolveMerchantAndCategoryAsync(PlaidTransaction plaidTransaction, NormalizationContext ctx)
	{
		var skipMerchantResolution = plaidTransaction.PersonalFinanceCategory is null ||
		                             _categoriesToIgnoreMerchantGeneration.Contains(plaidTransaction.PersonalFinanceCategory.Primary) ||
		                             string.IsNullOrEmpty(plaidTransaction.MerchantName);

		if (skipMerchantResolution)
		{
			return (ctx.OtherMerchant.Id, ctx.OtherUserMerchant.Id, ctx.OtherUserCategory.Id);
		}

		var predefinedMerchant = FindPredefinedMerchant(
			ctx.AllPredefinedMerchants,
			plaidTransaction.MerchantEntityId,
			plaidTransaction.Name);

		Merchant? merchant = null;
		if (!string.IsNullOrEmpty(plaidTransaction.MerchantEntityId))
		{
			merchant = ctx.AllMerchants.SingleOrDefault(p => p.PlaidId == plaidTransaction.MerchantEntityId);
		}
		if (merchant == null)
		{
			merchant = await CreateMerchantAsync(plaidTransaction, predefinedMerchant, ctx);
		}

		var (userMerchantId, userCategoryId) = await ResolveUserMerchantAndCategoryAsync(plaidTransaction,
			merchant,
			predefinedMerchant,
			ctx);

		return (merchant.Id, userMerchantId, userCategoryId);
	}

	private async Task<Merchant> CreateMerchantAsync(PlaidTransaction plaidTransaction, PredefinedMerchant? predefinedMerchant, NormalizationContext ctx)
	{
		var merchant = new Merchant
		{
			CategoryId = predefinedMerchant?.CategoryId ?? ctx.OtherCategory.Id,
			Name = plaidTransaction.MerchantName ?? plaidTransaction.Name,
			PlaidId = plaidTransaction.MerchantEntityId,
		};
		await merchantRepository.InsertAsync(merchant);
		ctx.AllMerchants.Add(merchant);
		return merchant;
	}

	private async Task<(ObjectId userMerchantId, ObjectId userCategoryId)> ResolveUserMerchantAndCategoryAsync(PlaidTransaction plaidTransaction,
		Merchant merchant,
		PredefinedMerchant? predefinedMerchant,
		NormalizationContext ctx)
	{
		var userCategoryId = ResolveUserCategoryIdAsync(merchant, predefinedMerchant, ctx);

		var userMerchant = ctx.UserMerchants.SingleOrDefault(p => p.MerchantId == merchant.Id);

		if (userMerchant is null)
		{
			userMerchant = new UserMerchant
			{
				UserId = ctx.UserId,
				MerchantId = merchant.Id,
				UserCategoryId = userCategoryId,
				TotalTransactionAmount = plaidTransaction.Amount,
				TotalTransactionCount = 1,
			};
			await userMerchantRepository.InsertAsync(userMerchant);
			ctx.UserMerchants.Add(userMerchant);
		}
		else
		{
			userMerchant.TotalTransactionAmount += plaidTransaction.Amount;
			userMerchant.TotalTransactionCount++;
			userMerchant.UserCategoryId ??= userCategoryId;
			await userMerchantRepository.UpdateAsync(userMerchant);
			ctx.UserMerchants[ctx.UserMerchants.FindIndex(p => p.Id == userMerchant.Id)] = userMerchant;
		}

		return (userMerchant.Id, userCategoryId);
	}

	private ObjectId ResolveUserCategoryIdAsync(Merchant merchant, PredefinedMerchant? predefinedMerchant, NormalizationContext ctx)
	{
		if (predefinedMerchant?.CategoryId != null)
		{
			var userCategory = ctx.UserCategories.SingleOrDefault(p => p.CategoryId == predefinedMerchant.CategoryId);
			return userCategory?.Id ?? ctx.OtherCategory.Id;
		}

		var merchantUserCategory = ctx.UserCategories.SingleOrDefault(p => p.CategoryId == merchant.CategoryId);
		return merchantUserCategory?.Id ?? ctx.OtherCategory.Id;
	}

	private static PredefinedMerchant? FindPredefinedMerchant(List<PredefinedMerchant> allPredefinedMerchants, string? merchantEntityId, string transactionName)
	{
		if (!string.IsNullOrEmpty(merchantEntityId))
		{
			var predefinedMerchant = allPredefinedMerchants.FirstOrDefault(p => p.PlaidId == merchantEntityId);
			if (predefinedMerchant != null)
			{
				return predefinedMerchant;
			}
		}

		return allPredefinedMerchants.FirstOrDefault(p => string.Equals(p.Name.Trim(), transactionName.Trim(), StringComparison.OrdinalIgnoreCase));
	}


	private bool ShouldSkip(PlaidTransaction plaidTransaction, AccountNormalizationState? accountNormalizationState)
	{
		var logData = JsonSerializer.Serialize(new
			{plaidTransaction.AccountId, plaidTransaction.Amount, plaidTransaction.Date, plaidTransaction.Pending, plaidTransaction.Name, plaidTransaction.TransactionType});
		if (plaidTransaction.Pending)
		{
			logger.LogInformation("skipping transaction normalization, transaction is pending. plaidTransaction: {PlaidTransaction}", logData);
			return true;
		}
		
		if (accountNormalizationState!=null && accountNormalizationState.Date >= plaidTransaction.Date)
		{
			//Kullanici bankasini kaldirip tekrar eklediginda arada 90 gunden az varsa normalization transactioni duplicate etmemek icin state tutulur ve burda kontrol edilir. Eger ilk donemde ekli son gun 90 gunden once degilse normalization transaction tekrar eklenmez 
			//Ya da kullanici hesapta guncelleme yapmis olabilir, bu durumda eski hesabi yeniden secer ve sistem plaidden toplu sekilde datayi ceker ama tekrar normalize etmemesi gerekir cunku o data zaten var
			
			logger.LogInformation("skipping transaction normalization, transaction already normalized. plaidTransaction: {PlaidTransaction}",logData);
			return true;
		}

		// Negatif amount para girişi, refund değilse ignore edilir
		var isRefund = plaidTransaction.Name.Contains("refund", StringComparison.OrdinalIgnoreCase) ||
		               plaidTransaction.TransactionType?.Contains("refund", StringComparison.OrdinalIgnoreCase) == true;

		if (isRefund)
		{
			logger.LogInformation("skipping transaction normalization, transaction is refund. plaidTransaction: {PlaidTransaction}", logData);
			return true;
		}

		if (plaidTransaction.Amount < 0)
		{
			logger.LogInformation("skipping transaction normalization, transaction amount is less than 0 which indicates the payment not expense. plaidTransaction: {PlaidTransaction}", logData);
			return true;
		}

		return false;
	}

	private static void ValidateRawTransaction(RawTransaction rawTransaction)
	{
		if (rawTransaction.PlaidTransactionsGetResponse.Account is null)
			throw new Exception($"Account is null. RawTransactionId: {rawTransaction.Id}");
	}

	private static void ValidatePlaidTransaction(RawTransaction rawTransaction, PlaidTransaction plaidTransaction)
	{
		if (string.IsNullOrEmpty(plaidTransaction.AccountId))
			throw new Exception( //TODO alarm email
				$"Transaction account id is empty. " +
				$"rawTransactionId: {rawTransaction.Id} " +
				$"PlaidTransactionId: {plaidTransaction.TransactionId}");
	}


	private async Task<List<RawTransaction>> SaveAllTransactionResponses(List<PlaidTransactionsGetResponseViewModel> allTransactionResponses,
		DateOnly startDate,
		DateOnly endDate,
		ObjectId userId,
		List<string> newAccountPlaidIds)
	{
		var daysToProcess = (endDate.ToDateTime(TimeOnly.MinValue) - startDate.ToDateTime(TimeOnly.MinValue)).Days + 1;

		var rawTransactions = new List<RawTransaction>();

		for (var i = 0; i < daysToProcess; i++)
		{
			var date = startDate.AddDays(i);

			var plaidTransactionGetResponse = allTransactionResponses.SingleOrDefault(p => p.Transactions.Any(t => t.Date == date));

			if (plaidTransactionGetResponse is null)
			{
				continue;
			}

			var plaidTransactionsInDate = plaidTransactionGetResponse.Transactions
				.Where(p => p.Date == date && (newAccountPlaidIds.Count == 0 || newAccountPlaidIds.Contains(p.AccountId)))
				.ToList();

			if (!plaidTransactionsInDate.Any())
			{
				continue;
			}

			var accountGroups = plaidTransactionsInDate.GroupBy(t => t.AccountId);

			foreach (var accountGroup in accountGroups)
			{
				var plaidAccountId = accountGroup.Key;
				var plaidAccount = plaidTransactionGetResponse.Accounts?.SingleOrDefault(p => p.AccountId == plaidAccountId);

				var plaidTransactionsGetResponse = new PlaidTransactionsGetResponse
				{
					Account = plaidAccount != null ? MapPlaidTransactionAccountFromViewModel(plaidAccount) : null,
					Transactions = accountGroup.Select(MapPlaidTransactionFromViewModel),
					Item = plaidTransactionGetResponse.Item != null ? MapPlaidTransactionItemFromViewModel(plaidTransactionGetResponse.Item) : null,
					RequestId = plaidTransactionGetResponse.RequestId,
					TotalTransactions = accountGroup.Count(),
				};

				var rawTransaction = new RawTransaction
				{
					UserId = userId,
					Date = date,
					Initial = true,
					PlaidAccountId = plaidAccountId,
					PlaidTransactionsGetResponse = plaidTransactionsGetResponse
				};

				rawTransactions.Add(rawTransaction);
			}
		}

		if (rawTransactions.Any())
		{
			await rawTransactionRepository.InsertManyAsync(rawTransactions);
			return rawTransactions;
		}
		return [];
	}


	public async Task<List<PlaidTransactionsGetResponseViewModel>> GetAllTransactionsAsync(string accessToken,
		DateOnly startDate,
		DateOnly endDate,
		ObjectId userId,
		List<string> requestNewAccountPlaidIds)
	{
		var allPages = new List<PlaidTransactionsGetResponseViewModel>();
		var offset = 0;
		var count = 500;
		var totalTransactions = 0;

		do
		{
			var request = new
			{
				client_id = plaidSettings.ClientId,
				secret = plaidSettings.Secret,
				access_token = accessToken,
				start_date = startDate.ToString("yyyy-MM-dd"),
				end_date = endDate.ToString("yyyy-MM-dd"), //The transaction in the end date are included
				options = new
				{
					count = count,
					offset = offset,
					account_ids = requestNewAccountPlaidIds,
				},
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
				Content = responseContent.Length > 500 ? responseContent.Substring(0, 100) : responseContent, // Büyük veri için opsiyonel
				Type = PlaidCommunicationLogType.GetTransactionsResponse
			});

			if (!response.IsSuccessStatusCode)
			{
				logger.LogError(
					"Plaid transactions/get failed for user {UserId} status {Status}. Response: {Response}",
					userId,
					response.StatusCode,
					responseContent);

				var plaidErrorModel = JsonSerializer.Deserialize<PlaidErrorViewModel>(responseContent);
				if (plaidErrorModel?.ErrorCode == "PRODUCT_NOT_READY")
				{
					break;
				}

				//webhook
				//{ "display_message" : null, "documentation_url" : "https://plaid.com/docs/?ref=error#item-errors", "error_code" : "PRODUCT_NOT_READY", "error_message" : "the requested product is not yet ready. please provide a webhook or try the request again later", "error_type" : "ITEM_ERROR", "request_id" : "Lxs5I67kSsOWFZR", "suggested_action" : null }

				throw new Exception($"Plaid transactions/get failed. Response: {responseContent}");
			}

			var pageResult = JsonSerializer.Deserialize<PlaidTransactionsGetResponseViewModel>(responseContent)!;
			allPages.Add(pageResult);

			totalTransactions = pageResult.TotalTransactions;
			offset += count;

		}
		while (offset < totalTransactions);

		return allPages;
	}

	private PlaidTransactionItem MapPlaidTransactionItemFromViewModel(PlaidTransactionItemViewModel viewModel)
	{
		PlaidTransactionError? error = null;
		if (viewModel.Error != null)
		{
			List<PlaidTransactionErrorCause>? causes = null;
			if (viewModel.Error.Causes != null)
			{
				causes = new List<PlaidTransactionErrorCause>();
				foreach (var cause in viewModel.Error.Causes)
				{
					causes.Add(new PlaidTransactionErrorCause
					{
						DisplayMessage = cause.DisplayMessage,
						ErrorCode = cause.ErrorCode,
						ErrorMessage = cause.ErrorMessage,
						ErrorType = cause.ErrorType,
						ItemId = cause.ItemId,
					});
				}
			}

			error = new PlaidTransactionError
			{
				Causes = causes,
				DisplayMessage = viewModel.Error.DisplayMessage,
				DocumentationUrl = viewModel.Error.DocumentationUrl,
				ErrorMessage = viewModel.Error.ErrorMessage,
				ErrorCode = viewModel.Error.ErrorCode,
				ErrorType = viewModel.Error.ErrorType,
				RequestId = viewModel.Error.RequestId,
				Status = viewModel.Error.Status,
				SuggestedAction = viewModel.Error.SuggestedAction,
			};
		}

		return new PlaidTransactionItem
		{
			AvailableProducts = viewModel.AvailableProducts,
			BilledProducts = viewModel.BilledProducts,
			ConsentExpirationTime = viewModel.ConsentExpirationTime,
			Error = error,
			InstitutionId = viewModel.InstitutionId,
			ItemId = viewModel.ItemId,
			Products = viewModel.Products,
			UpdateType = viewModel.UpdateType,
			Webhook = viewModel.Webhook,
		};
	}

	private PlaidTransactionAccount MapPlaidTransactionAccountFromViewModel(PlaidTransactionAccountViewModel viewModel)
	{
		PlaidTransactionAccountBalances? balances = null;
		if (viewModel.Balances != null)
		{
			balances = new PlaidTransactionAccountBalances
			{
				Available = viewModel.Balances.Available,
				Current = viewModel.Balances.Current,
				IsoCurrencyCode = viewModel.Balances.IsoCurrencyCode,
				Limit = viewModel.Balances.Limit,
				UnofficialCurrencyCode = viewModel.Balances.UnofficialCurrencyCode,
			};
		}

		return new PlaidTransactionAccount
		{
			AccountId = viewModel.AccountId,
			Balances = balances,
			Mask = viewModel.Mask,
			Name = viewModel.Name,
			Type = viewModel.Type,
			OfficialName = viewModel.OfficialName,
			Subtype = viewModel.Subtype,
		};
	}


	private PlaidTransaction MapPlaidTransactionFromViewModel(PlaidTransactionViewModel viewModel)
	{
		PlaidTransactionLocation? location = null;
		if (viewModel.Location != null)
		{
			location = new PlaidTransactionLocation
			{
				Address = viewModel.Location.Address,
				City = viewModel.Location.City,
				Country = viewModel.Location.Country,
				Lat = viewModel.Location.Lat,
				Lon = viewModel.Location.Lon,
				PostalCode = viewModel.Location.PostalCode,
				Region = viewModel.Location.Region,
				StoreNumber = viewModel.Location.StoreNumber,
			};
		}

		List<PlaidTransactionCounterparty>? counterparties = null;
		if (viewModel.Counterparties != null)
		{
			counterparties = new List<PlaidTransactionCounterparty>();
			foreach (var counterpartyViewModel in viewModel.Counterparties)
			{
				counterparties.Add(new PlaidTransactionCounterparty
				{
					ConfidenceLevel = counterpartyViewModel.ConfidenceLevel,
					EntityId = counterpartyViewModel.EntityId,
					LogoUrl = counterpartyViewModel.LogoUrl,
					Name = counterpartyViewModel.Name,
					Type = counterpartyViewModel.Type,
					Website = counterpartyViewModel.Website,
				});
			}
		}

		PlaidTransactionPaymentMeta? paymentMeta = null;
		if (viewModel.PaymentMeta != null)
		{
			paymentMeta = new PlaidTransactionPaymentMeta
			{
				ByOrderOf = viewModel.PaymentMeta.ByOrderOf,
				Payee = viewModel.PaymentMeta.Payee,
				Payer = viewModel.PaymentMeta.Payer,
				PaymentMethod = viewModel.PaymentMeta.PaymentMethod,
				PaymentProcessor = viewModel.PaymentMeta.PaymentProcessor,
				PpdId = viewModel.PaymentMeta.PpdId,
				Reason = viewModel.PaymentMeta.Reason,
				Receiver = viewModel.PaymentMeta.Receiver,
				ReferenceNumber = viewModel.PaymentMeta.ReferenceNumber,
			};
		}

		PlaidTransactionPersonalFinanceCategory? personalFinanceCategory = null;
		if (viewModel.PersonalFinanceCategory != null)
		{
			personalFinanceCategory = new PlaidTransactionPersonalFinanceCategory
			{
				ConfidenceLevel = viewModel.PersonalFinanceCategory.ConfidenceLevel,
				Detailed = viewModel.PersonalFinanceCategory.Detailed,
				Primary = viewModel.PersonalFinanceCategory.Primary,
			};
		}


		return new PlaidTransaction
		{
			AccountId = viewModel.AccountId,
			Amount = viewModel.Amount,
			AccountOwner = viewModel.AccountOwner,
			AuthorizedDate = viewModel.AuthorizedDate,
			AuthorizedDateTime = viewModel.AuthorizedDateTime,
			Category = viewModel.Category,
			CategoryId = viewModel.CategoryId,
			CheckNumber = viewModel.CheckNumber,
			Counterparties = counterparties,
			Date = viewModel.Date,
			DateTime = viewModel.DateTime,
			IsoCurrencyCode = viewModel.IsoCurrencyCode,
			Location = location,
			LogoUrl = viewModel.LogoUrl,
			MerchantEntityId = viewModel.MerchantEntityId,
			MerchantName = viewModel.MerchantName,
			Name = viewModel.Name,
			PaymentChannel = viewModel.PaymentChannel,
			PaymentMeta = paymentMeta,
			Pending = viewModel.Pending, //Pending data is ignored when processing
			PendingTransactionId = viewModel.PendingTransactionId,
			PersonalFinanceCategory = personalFinanceCategory,
			PersonalFinanceCategoryIconUrl = viewModel.PersonalFinanceCategoryIconUrl,
			TransactionCode = viewModel.TransactionCode,
			TransactionId = viewModel.TransactionId,
			TransactionType = viewModel.TransactionType,
			UnofficialCurrencyCode = viewModel.UnofficialCurrencyCode,
			Website = viewModel.Website,
		};
	}
}

public sealed record NormalizationContext(
	ObjectId UserId,
	List<Merchant> AllMerchants,
	List<PredefinedMerchant> AllPredefinedMerchants,
	Merchant OtherMerchant,
	UserMerchant OtherUserMerchant,
	UserCategory OtherUserCategory,
	Category OtherCategory,
	List<UserCategory> UserCategories,
	List<UserMerchant> UserMerchants);
