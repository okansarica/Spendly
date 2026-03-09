namespace Spendly.Shared.BusinessLayer;

using Core;
using DataLayer;
using Entities.Banking;
using Entities.Reporting;
using Entities.TransactionManagement;
using Entities.UserManagement;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using System.Net.Http.Json;
using System.Text.Json;
using ViewModels.Plaid;
using ViewModels.Settings;

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
	IRepository<DailyAccountExpense> dailyAccountExpenseRepository,
	IRepository<DailyCategoryExpense> dailyCategoryExpenseRepository,
	IRepository<DailyCategoryAccountExpense> dailyCategoryAccountExpenseRepository,
	IRepository<MonthlyUserExpense> monthlyUserExpenseRepository,
	IRepository<CategoryMonthlyExpense> categoryMonthlyExpenseRepository,
	IRepository<MerchantMonthlyExpense> merchantMonthlyExpenseRepository)
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
			request.UserId.ToObjectId());

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
			[]);

		//normalize et
		var normalizedTransactions = await NormalizeTransactions(rawTransactions, request.BankId.ToObjectId(), request.UserId.ToObjectId());

		//rapor tablolarini doldur
		await CreateReportingData(normalizedTransactions,  request.UserId.ToObjectId());
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

		await CreateDailyUserExpenses(normalizedTransactions, userId);
		await CreateDailyAccountExpenses(normalizedTransactions, userId, accountIdToBankIdMap);
		await CreateDailyCategoryExpenses(normalizedTransactions, userId);
		await CreateDailyCategoryAccountExpenses(normalizedTransactions, userId, accountIdToBankIdMap);
		await CreateMonthlyUserExpenses(normalizedTransactions, userId);
		await CreateMonthlyCategoryExpenses(normalizedTransactions);
		await CreateMonthlyMerchantExpenses(normalizedTransactions);
	}

	private async Task CreateDailyUserExpenses(List<NormalizedTransaction> normalizedTransactions, ObjectId userId)
	{
		var groupedData = normalizedTransactions
			.GroupBy(p => p.DateTime.Date)
			.Select(g => new DailyUserExpense
			{
				UserId = userId,
				DateTime = g.Key,
				TotalAmount = g.Sum(t => t.Amount)
			})
			.ToList();

		if (groupedData.Any())
		{
			await dailyUserExpenseRepository.InsertManyAsync(groupedData);
		}
	}

	private async Task CreateDailyAccountExpenses(List<NormalizedTransaction> normalizedTransactions, ObjectId userId, Dictionary<ObjectId, ObjectId> accountIdToBankIdMap)
	{
		var groupedData = normalizedTransactions
			.GroupBy(p => new { p.AccountId, Date = p.DateTime.Date })
			.Select(g => new DailyAccountExpense
			{
				UserId = userId,
				DateTime = g.Key.Date,
				BankId = accountIdToBankIdMap[g.Key.AccountId],
				AccountId = g.Key.AccountId,
				TotalAmount = g.Sum(t => t.Amount)
			})
			.ToList();

		if (groupedData.Any())
		{
			await dailyAccountExpenseRepository.InsertManyAsync(groupedData);
		}
	}

	private async Task CreateDailyCategoryExpenses(List<NormalizedTransaction> normalizedTransactions, ObjectId userId)
	{
		var groupedData = normalizedTransactions
			.Where(p => p.UserCategoryId.HasValue)
			.GroupBy(p => new { p.UserCategoryId, Date = p.DateTime.Date })
			.Select(g => new DailyCategoryExpense
			{
				UserId = userId,
				CategoryId = g.Key.UserCategoryId!.Value,
				DateTime = g.Key.Date,
				TotalAmount = g.Sum(t => t.Amount)
			})
			.ToList();

		if (groupedData.Any())
		{
			await dailyCategoryExpenseRepository.InsertManyAsync(groupedData);
		}
	}

	private async Task CreateDailyCategoryAccountExpenses(List<NormalizedTransaction> normalizedTransactions, ObjectId userId, Dictionary<ObjectId, ObjectId> accountIdToBankIdMap)
	{
		var groupedData = normalizedTransactions
			.Where(p => p.UserCategoryId.HasValue)
			.GroupBy(p => new { p.UserCategoryId, p.AccountId, Date = p.DateTime.Date })
			.Select(g => new DailyCategoryAccountExpense
			{
				UserId = userId,
				Date = g.Key.Date,
				BankId = accountIdToBankIdMap[g.Key.AccountId],
				CategoryId = g.Key.UserCategoryId!.Value,
				AccountId = g.Key.AccountId,
				TotalAmount = g.Sum(t => t.Amount)
			})
			.ToList();

		if (groupedData.Any())
		{
			await dailyCategoryAccountExpenseRepository.InsertManyAsync(groupedData);
		}
	}

	private async Task CreateMonthlyUserExpenses(List<NormalizedTransaction> normalizedTransactions, ObjectId userId)
	{
		var groupedData = normalizedTransactions
			.GroupBy(p => new { p.DateTime.Year, p.DateTime.Month })
			.Select(g => new MonthlyUserExpense
			{
				UserId = userId,
				Year = g.Key.Year,
				Month = g.Key.Month,
				TotalAmount = g.Sum(t => t.Amount)
			})
			.ToList();

		if (groupedData.Any())
		{
			await monthlyUserExpenseRepository.InsertManyAsync(groupedData);
		}
	}

	private async Task CreateMonthlyCategoryExpenses(List<NormalizedTransaction> normalizedTransactions)
	{
		var groupedData = normalizedTransactions
			.Where(p => p.UserCategoryId.HasValue)
			.GroupBy(p => new { p.UserCategoryId, p.DateTime.Year, p.DateTime.Month })
			.Select(g => new CategoryMonthlyExpense
			{
				CategoryId = g.Key.UserCategoryId!.Value,
				Year = g.Key.Year,
				Month = g.Key.Month,
				TransactionCount = g.Count(),
				TransactionAmount = g.Sum(t => t.Amount)
			})
			.ToList();

		if (groupedData.Any())
		{
			await categoryMonthlyExpenseRepository.InsertManyAsync(groupedData);
		}
	}

	private async Task CreateMonthlyMerchantExpenses(List<NormalizedTransaction> normalizedTransactions)
	{
		var groupedData = normalizedTransactions
			.Where(p => p.MerchantId.HasValue)
			.GroupBy(p => new { p.MerchantId, p.DateTime.Year, p.DateTime.Month })
			.Select(g => new MerchantMonthlyExpense
			{
				MerchantId = g.Key.MerchantId!.Value,
				Year = g.Key.Year,
				Month = g.Key.Month,
				TransactionCount = g.Count(),
				TransactionAmount = g.Sum(t => t.Amount)
			})
			.ToList();

		if (groupedData.Any())
		{
			await merchantMonthlyExpenseRepository.InsertManyAsync(groupedData);
		}
	}

	private async Task<List<NormalizedTransaction>> NormalizeTransactions(List<RawTransaction> rawTransactions, ObjectId bankId, ObjectId userId)
	{
		var allAccounts = await accountRepository.ListAsync(p => p.BankId == bankId);
		var allMerchants = await merchantRepository.ListAsync(filter:null);

		var normalizedTransactions = new List<NormalizedTransaction>();
		
		foreach (var rawTransaction in rawTransactions)
		{
			foreach (var plaidTransaction in rawTransaction.PlaidTransactionsGetResponse.Transactions)
			{
				if (plaidTransaction.Pending)
				{
					//Pending transactionlar complete olunca complete olduklari gunun listesinde cikiyor
					continue;
				}
				
				if (plaidTransaction.Amount<0 &&
				    (!plaidTransaction.Name.Contains("refund") || plaidTransaction.TransactionType?.Contains("refund") != true))
				{
					// - amount para girisi, ignore edilir
					continue;
				}
				if (rawTransaction.PlaidTransactionsGetResponse.Accounts is null)
				{
					throw new Exception($"Account list is null. RawTransactionId: {rawTransaction.Id}");
				}
				if (string.IsNullOrEmpty(plaidTransaction.AccountId))
				{
					//TODO alarm email
					throw new Exception($"Transaction account id is empty. rawTransactionId: {rawTransaction.Id} PlaidTransactionId: {plaidTransaction.TransactionId}");
				}

				var plaidAccount = rawTransaction.PlaidTransactionsGetResponse.Accounts.Single(p => p.AccountId == plaidTransaction.AccountId);
				
				var account = allAccounts.Single(p=>p.PlaidAccountId == plaidAccount.AccountId);

				Merchant? merchant = null;
				ObjectId? userCategoryId = null;
				if (plaidTransaction.PersonalFinanceCategory is null || _categoriesToIgnoreMerchantGeneration.Contains(plaidTransaction.PersonalFinanceCategory.Primary))
				{
					//skip merchant generation
				}
				else if (!string.IsNullOrEmpty(plaidTransaction.MerchantName))
				{
					if (!string.IsNullOrEmpty(plaidTransaction.MerchantEntityId))
					{
						merchant = allMerchants.SingleOrDefault(p=>p.PlaidId == plaidTransaction.MerchantEntityId);	
					}
					if (merchant == null)
					{
						merchant = new Merchant
						{
							CategoryId = null, //yeni gelen merchant icin kategoriyi bilemeyiz
							Name = plaidTransaction.MerchantName!,
							PlaidId = plaidTransaction.MerchantEntityId,
						};
						await merchantRepository.InsertAsync(merchant);
						allMerchants.Add(merchant);

						var userMerchant = new UserMerchant
						{
							UserId = userId,
							UserCategoryId = null, //yeni gelen merchant icin kategoriyi bilemeyiz
							MerchantId = merchant.Id,
							TotalTransactionAmount = 0,
							TotalTransactionCount = 0,
						};
						await userMerchantRepository.InsertAsync(userMerchant);
					}
					else
					{
						if (merchant.CategoryId.HasValue)
						{
							var userCategory = await userCategoryRepository.GetAsync(p => p.CategoryId == merchant.CategoryId.Value);
							userCategoryId = userCategory?.Id;
						}
						
						var userMerchant = await userMerchantRepository.GetAsync(p=>p.UserId == userId && p.MerchantId == merchant.Id);
						if (userMerchant is null)
						{
							userMerchant = new UserMerchant
							{
								UserId = userId,
								UserCategoryId = userCategoryId,
								MerchantId = merchant.Id,
								TotalTransactionAmount = plaidTransaction.Amount,
								TotalTransactionCount = 1,
							};
							await userMerchantRepository.InsertAsync(userMerchant);
						}
						else
						{
							userMerchant.TotalTransactionAmount+=plaidTransaction.Amount;
							userMerchant.TotalTransactionCount++;
							if (!userMerchant.UserCategoryId.HasValue)
							{
								userMerchant.UserCategoryId = userCategoryId;
							}
							await  userMerchantRepository.UpdateAsync(userMerchant);
						}
					}
				}
				
				var normalizedTransaction = new NormalizedTransaction
				{
					AccountId = account.Id,
					Amount = plaidTransaction.Amount,
					DateTime = plaidTransaction.DateTime ?? plaidTransaction.Date.ToDateTime(TimeOnly.MinValue),
					MerchantId = merchant?.Id,
					PlaidTransactionId = plaidTransaction.TransactionId,
					RawTransactionId = rawTransaction.Id,
					UserId = userId,
					UserCategoryId = userCategoryId,
					TransactionName = plaidTransaction.Name
				};
				normalizedTransactions.Add(normalizedTransaction);
			}
		}

		if (normalizedTransactions.Any())
		{
			await normalizedTransactionRepository.InsertManyAsync(normalizedTransactions);
		}
		return normalizedTransactions;
	}

	private async Task<List<RawTransaction>> SaveAllTransactionResponses(List<PlaidTransactionsGetResponseViewModel> allTransactionResponses,
		DateOnly startDate,
		DateOnly endDate,
		ObjectId userId,
		List<string> newAccountPlaidIds)
	{
		var daysToProcess = (endDate.ToDateTime(TimeOnly.MinValue) - startDate.ToDateTime(TimeOnly.MinValue)).Days + 1; //+1 is to include the last date

		var rawTransactions = new List<RawTransaction>();

		//Her gun icin transactionlari al parcala kaydet
		for (var i = 0; i < daysToProcess; i++)
		{
			var date = startDate.AddDays(i);

			var plaidTransactionGetResponse = allTransactionResponses.SingleOrDefault(p => p.Transactions.Any(t => t.Date == date));

			if (plaidTransactionGetResponse is null)
			{
				//This means there is no transaction for that day
				continue;
			}

			// Her yeni banka baglantisi yapildiginda newAccountPlaidIds callback sonucunda gelir. Zaten var olan bir banka tekrar baglanmak istenebilir (varolan account cikarilabilir ya da yeni account eklenebilir. newAccountPlaidIds sadece eklenen 
			var plaidTransactionsInDate = plaidTransactionGetResponse.Transactions.Where(p => p.Date == date && !newAccountPlaidIds.Contains(p.AccountId)).ToList();

			var plaidAccountsForTransactions = plaidTransactionGetResponse.Accounts?.Where(p => plaidTransactionsInDate.Select(t => t.AccountId).Contains(p.AccountId)).ToList();

			var plaidTransactionsGetResponse = new PlaidTransactionsGetResponse
			{
				Accounts = plaidAccountsForTransactions?.Select(MapPlaidTransactionAccountFromViewModel).ToList(),
				Transactions = plaidTransactionsInDate.Select(MapPlaidTransactionFromViewModel),
				Item = plaidTransactionGetResponse.Item != null ? MapPlaidTransactionItemFromViewModel(plaidTransactionGetResponse.Item) : null,
				RequestId = plaidTransactionGetResponse.RequestId,
				TotalTransactions = plaidTransactionGetResponse.TotalTransactions,
			};

			var rawTransaction = new RawTransaction
			{
				UserId = userId,
				Date = date,
				Initial = true,
				PlaidTransactionsGetResponse = plaidTransactionsGetResponse
			};

			rawTransactions.Add(rawTransaction);
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
		ObjectId userId)
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
					offset = offset
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
				Content = responseContent.Length>500?responseContent.Substring(0,100):responseContent, // Büyük veri için opsiyonel
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
