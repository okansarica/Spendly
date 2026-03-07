namespace Spendly.Shared.BusinessLayer;

using DataLayer;
using Entities.Banking;
using Entities.TransactionManagement;
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
	IRepository<PlaidCommunicationLog> plaidCommunicationLogRepository,
	IRepository<UserPlaidToken> userPlaidTokenRepository,
	IDataProtectionProvider dataProtectionProvider,
	IRepository<RawTransaction> rawTransactionRepository)
{
	public async Task QueryAndSaveUserTransactionAsync(DateOnly startDate,
		DateOnly endDate,
		ObjectId userId)
	{
		//TODO eger tek gunlukse direk kaydet 
		
		var userPlaidToken = await userPlaidTokenRepository.GetRequiredAsync(p => p.UserId == userId);
		
		var protector = dataProtectionProvider.CreateProtector("UserPlaidTokenProtector");
		var accessToken = protector.Unprotect(userPlaidToken.EncryptedAccessToken);

		var allTransactionResponses = await GetAllTransactionsAsync(accessToken,
			startDate,
			endDate,
			userId);

		await SaveAllTransactionResponses(allTransactionResponses,
			startDate,
			endDate,
			userId);
	}
	private async Task SaveAllTransactionResponses(List<PlaidTransactionsGetResponseViewModel> allTransactionResponses,
		DateOnly startDate,
		DateOnly endDate,
		ObjectId userId)
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

			var plaidTransactionsInDate = plaidTransactionGetResponse.Transactions.Where(p => p.Date == date).ToList();

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
		}
	}


	/// <summary>
	/// Access token sayesinde
	/// </summary>
	/// <param name="accessToken"></param>
	/// <param name="startDate"></param>
	/// <param name="endDate"></param>
	/// <param name="userId"></param>
	/// <returns></returns>
	/// <exception cref="Exception"></exception>
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
				Content = responseContent, // Büyük veri için opsiyonel
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
