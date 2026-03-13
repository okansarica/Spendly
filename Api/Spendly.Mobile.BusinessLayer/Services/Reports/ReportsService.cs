// CHANGED_BY_AI: 2026-03-02 - Add reports service
namespace Spendly.Mobile.BusinessLayer.Services.Reports;

using MongoDB.Bson;
using MongoDB.Driver;
using Shared.Core.Extensions;
using Spendly.Mobile.BusinessLayer.Constants;
using Spendly.Mobile.ViewModels.Reports;
using Spendly.Shared.Core;
using Spendly.Shared.DataLayer;
using Spendly.Shared.Entities.Reporting;
using Spendly.Shared.Entities.TransactionManagement;
using Spendly.Shared.Entities.UserManagement;
using Spendly.Shared.Localization;
using Spendly.Shared.ViewModels;

public class ReportsService(
	IRepository<DailyCategoryAccountExpense> dailySummaryRepository,
	IRepository<NormalizedTransaction> transactionRepository,
	IRepository<Account> accountRepository,
	IRepository<UserCategory> categoryRepository,
	IRepository<UserMerchant> userMerchantRepository,
	IRepository<Merchant> merchantRepository,
	RequestContextViewModel requestContextViewModel)
{
	public async Task<FunctionResponse<ReportsOverviewResponseViewModel>> GetOverviewAsync(ReportsOverviewRequestViewModel request)
	{
		if (!TryResolveTimezone(requestContextViewModel.Timezone, out var tz))
		{
			return FunctionResponse.Failure<ReportsOverviewResponseViewModel>(MessageCodes.InvalidTimezone);
		}

		var nowLocal = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz).Date;
		var (startLocal, endLocal) = ResolveRange(request.StartDate, request.EndDate, nowLocal);
		var (prevStartLocal, prevEndLocal) = GetPreviousMonthSamePeriod(startLocal, endLocal);

		var (startUtc, endUtc) = ToUtcRange(startLocal, endLocal, tz);
		var (prevStartUtc, prevEndUtc) = ToUtcRange(prevStartLocal, prevEndLocal, tz);

		var userId = requestContextViewModel.UserId.ToObjectId();
		var currentSummaries = await GetDailySummariesAsync(userId, startUtc, endUtc);
		var previousSummaries = await GetDailySummariesAsync(userId, prevStartUtc, prevEndUtc);

		var currentTotals = currentSummaries
			.GroupBy(x => x.CategoryId)
			.ToDictionary(g => g.Key, g => g.Sum(x => x.TotalAmount));

		var previousTotals = previousSummaries
			.GroupBy(x => x.CategoryId)
			.ToDictionary(g => g.Key, g => g.Sum(x => x.TotalAmount));

		var allCategoryIds = currentTotals.Keys
			.Concat(previousTotals.Keys)
			.Distinct()
			.ToList();

		var categoryLookup = await GetCategoryLookupAsync(allCategoryIds);

		var currentTotal = currentTotals.Values.Sum();
		var previousTotal = previousTotals.Values.Sum();

		var summary = BuildSummary(currentTotal, previousTotal);

		var categoryChanges = allCategoryIds
			.Select(id => BuildCategoryChange(id,
				currentTotals,
				previousTotals,
				categoryLookup))
			.OrderByDescending(x => Math.Abs(x.DifferenceAmount))
			.ToList();

		var topChanging = categoryChanges
			.Take(Constants.Reports.TopChangingCategories)
			.ToList();

		var distribution = currentTotals
			.Where(x => x.Value > 0)
			.Select(kvp => new ReportCategoryDistributionViewModel
			{
				CategoryId = kvp.Key.ToString(),
				CategoryName = categoryLookup[kvp.Key].Name,
				CurrentMonthToDateTotal = kvp.Value,
				PercentageOfTotal = currentTotal > 0 ? Math.Round(kvp.Value / currentTotal * 100, 1) : 0
			})
			.OrderByDescending(x => x.CurrentMonthToDateTotal)
			.ToList();

		var response = new ReportsOverviewResponseViewModel
		{
			Summary = summary,
			TopChangingCategories = topChanging,
			CategoryDistribution = distribution,
			Categories = categoryChanges.OrderByDescending(x => x.CurrentMonthToDateTotal).ToList()
		};

		return FunctionResponse.Success(response);
	}

	public async Task<FunctionResponse<ReportCategoryDetailResponseViewModel>> GetCategoryDetailAsync(string categoryId,
		ReportsCategoryRequestViewModel request)
	{
		var categoryObjectId = categoryId.ToObjectIdOrNull();
		if (categoryObjectId == null)
		{
			return FunctionResponse.Failure<ReportCategoryDetailResponseViewModel>(MessageCodes.InvalidCategoryId);
		}

		if (!TryResolveTimezone(requestContextViewModel.Timezone, out var tz))
		{
			return FunctionResponse.Failure<ReportCategoryDetailResponseViewModel>(MessageCodes.InvalidTimezone);
		}

		var nowLocal = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz).Date;
		var (startLocal, endLocal) = ResolveRange(request.StartDate, request.EndDate, nowLocal);
		var (startUtc, endUtc) = ToUtcRange(startLocal, endLocal, tz);

		var accountId = request.AccountId?.ToObjectIdOrNull();

		var userId = requestContextViewModel.UserId.ToObjectId();
		var categoryTotals = await GetDailySummariesAsync(userId,
			startUtc,
			endUtc,
			categoryObjectId,
			accountId);
		var totalAmount = categoryTotals.Sum(x => x.TotalAmount);

		var categoryName = await GetCategoryNameAsync(categoryObjectId.Value);

		var page = request.Page ?? 1;
		var pageSize = request.PageSize ?? Constants.Reports.DefaultPageSize;

		var (transactions, total) = await GetCategoryTransactionsAsync(
			userId,
			categoryObjectId.Value,
			accountId,
			startUtc,
			endUtc,
			page,
			pageSize);

		var accountLookup = await GetAccountLookupAsync(transactions.Select(x => x.AccountId).Distinct().ToList());

		var merchantIds = transactions.Select(x => x.MerchantId).Distinct().ToList();
		var userMerchantLookup = await GetMerchantLookupAsync(merchantIds);

		var allMerchants = await merchantRepository.ListAsync(merchantIds);

		var items = new List<ReportTransactionItemViewModel>();

		foreach (var transaction in transactions)
		{
			string? merchantName;

			var userMerchant = userMerchantLookup[transaction.MerchantId];
			if (!string.IsNullOrEmpty(userMerchant.Nickname))
			{
				merchantName = userMerchant.Nickname;
			}
			else
			{
				var merchant = allMerchants.Single(x => x.Id == transaction.MerchantId);
				merchantName = merchant.Name;
			}


			items.Add(new ReportTransactionItemViewModel
			{
				TransactionId = transaction.Id.ToString(),
				Date = transaction.DateTime,
				MerchantName = merchantName,
				AccountName = accountLookup[transaction.AccountId].NickName ?? accountLookup[transaction.AccountId].Name,
				Amount = transaction.Amount,
				TransactionName = transaction.TransactionName
			});
		}


		var totalPages = pageSize > 0 ? (int) Math.Ceiling((double) total / pageSize) : 1;

		var response = new ReportCategoryDetailResponseViewModel
		{
			CategorySummary = new ReportCategorySummaryViewModel
			{
				CategoryId = categoryObjectId.Value.ToString(),
				CategoryName = categoryName,
				TotalAmount = totalAmount
			},
			Transactions = new ReportTransactionPageViewModel
			{
				Items = items,
				Total = (int) total,
				PageNumber = page,
				PageSize = pageSize,
				TotalPages = totalPages
			}
		};

		return FunctionResponse.Success(response);
	}

	public async Task<FunctionResponse<AccountsOverviewResponseViewModel>> GetAccountsOverviewAsync(AccountsOverviewRequestViewModel request)
	{
		if (!TryResolveTimezone(requestContextViewModel.Timezone, out var tz))
		{
			return FunctionResponse.Failure<AccountsOverviewResponseViewModel>(MessageCodes.InvalidTimezone);
		}

		var nowLocal = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz).Date;
		var (startLocal, endLocal) = ResolveRange(request.StartDate, request.EndDate, nowLocal);
		var (prevStartLocal, prevEndLocal) = GetPreviousMonthSamePeriod(startLocal, endLocal);

		var (startUtc, endUtc) = ToUtcRange(startLocal, endLocal, tz);
		var (prevStartUtc, prevEndUtc) = ToUtcRange(prevStartLocal, prevEndLocal, tz);

		var userId = requestContextViewModel.UserId.ToObjectId();
		var currentSummaries = await GetDailySummariesAsync(userId, startUtc, endUtc);
		var previousSummaries = await GetDailySummariesAsync(userId, prevStartUtc, prevEndUtc);

		var currentTotals = currentSummaries
			.GroupBy(x => x.AccountId)
			.ToDictionary(g => g.Key, g => g.Sum(x => x.TotalAmount));

		var previousTotals = previousSummaries
			.GroupBy(x => x.AccountId)
			.ToDictionary(g => g.Key, g => g.Sum(x => x.TotalAmount));

		var allAccountIds = currentTotals.Keys
			.Concat(previousTotals.Keys)
			.Distinct()
			.ToList();

		var accountLookup = await GetAccountLookupAsync(allAccountIds);

		var currentTotal = currentTotals.Values.Sum();
		var previousTotal = previousTotals.Values.Sum();

		var summary = BuildSummary(currentTotal, previousTotal);

		var accounts = allAccountIds
			.Select(id => BuildAccountListItem(id,
				currentTotals,
				previousTotals,
				accountLookup))
			.OrderByDescending(x => x.CurrentMonthToDateTotal)
			.ToList();

		var distribution = currentTotals
			.Where(x => x.Value > 0)
			.Select(kvp => new AccountDistributionViewModel
			{
				AccountId = kvp.Key.ToString(),
				AccountName = accountLookup[kvp.Key].NickName ?? accountLookup[kvp.Key].Name,
				CurrentMonthToDateTotal = kvp.Value,
				PercentageOfTotal = currentTotal > 0 ? Math.Round(kvp.Value / currentTotal * 100, 1) : 0
			})
			.OrderByDescending(x => x.CurrentMonthToDateTotal)
			.ToList();

		var response = new AccountsOverviewResponseViewModel
		{
			Summary = summary,
			AccountDistribution = distribution,
			Accounts = accounts
		};

		return FunctionResponse.Success(response);
	}

	public async Task<FunctionResponse<AccountDetailResponseViewModel>> GetAccountDetailAsync(string accountId,
		AccountDetailRequestViewModel request)
	{
		var accountObjectId = accountId.ToObjectId();

		if (!TryResolveTimezone(requestContextViewModel.Timezone, out var tz))
		{
			return FunctionResponse.Failure<AccountDetailResponseViewModel>(MessageCodes.InvalidTimezone);
		}

		var account = await accountRepository.GetRequiredAsync(accountObjectId);

		var nowLocal = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz).Date;
		var (startLocal, endLocal) = ResolveRange(request.StartDate, request.EndDate, nowLocal);
		var (startUtc, endUtc) = ToUtcRange(startLocal, endLocal, tz);

		var userId = requestContextViewModel.UserId.ToObjectId();
		var summaries = await GetDailySummariesAsync(userId,
			startUtc,
			endUtc,
			null,
			accountObjectId);
		var totalAmount = summaries.Sum(x => x.TotalAmount);

		var categories = summaries
			.GroupBy(x => x.CategoryId)
			.Select(g => new {CategoryId = g.Key, Total = g.Sum(x => x.TotalAmount)})
			.OrderByDescending(x => x.Total)
			.ToList();

		var categoryLookup = await GetCategoryLookupAsync(categories.Select(x => x.CategoryId).ToList());

		var categoryTotals = categories.Select(x => new AccountCategoryTotalViewModel
			{
				CategoryId = x.CategoryId.ToString(),
				CategoryName = categoryLookup[x.CategoryId].Name,
				TotalAmount = x.Total
			})
			.ToList();

		var comparisonResult = await BuildAccountComparison(
			startLocal,
			endLocal,
			nowLocal,
			userId,
			accountObjectId,
			tz,
			totalAmount);



		var response = new AccountDetailResponseViewModel
		{
			AccountSummary = new AccountSummaryViewModel
			{
				AccountId = accountObjectId.ToString(),
				AccountName = account.NickName ?? account.Name,
				StartDate = startLocal,
				EndDate = endLocal,
				TotalAmount = totalAmount,
				Comparison = comparisonResult
			},
			Categories = categoryTotals
		};

		return FunctionResponse.Success(response);
	}

	public async Task<FunctionResponse<MerchantsReportOverviewResponseViewModel>> GetMerchantsOverviewAsync(MerchantsReportOverviewRequestViewModel request)
	{
		if (!TryResolveTimezone(requestContextViewModel.Timezone, out var tz))
		{
			return FunctionResponse.Failure<MerchantsReportOverviewResponseViewModel>(MessageCodes.InvalidTimezone);
		}

		var nowLocal = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz).Date;
		var (startLocal, endLocal) = ResolveRange(request.StartDate, request.EndDate, nowLocal);
		var (prevStartLocal, prevEndLocal) = GetPreviousMonthSamePeriod(startLocal, endLocal);

		var (startUtc, endUtc) = ToUtcRange(startLocal, endLocal, tz);
		var (prevStartUtc, prevEndUtc) = ToUtcRange(prevStartLocal, prevEndLocal, tz);

		var userId = requestContextViewModel.UserId.ToObjectId();
		
		var currentTransactions = await GetMerchantTransactionTotalsAsync(userId, startUtc, endUtc);
		var previousTransactions = await GetMerchantTransactionTotalsAsync(userId, prevStartUtc, prevEndUtc);

		var currentMerchantTotals = currentTransactions
			.GroupBy(x => x.MerchantId)
			.ToDictionary(g => g.Key, g => g.Sum(x => x.Amount));

		var previousMerchantTotals = previousTransactions
			.GroupBy(x => x.MerchantId)
			.ToDictionary(g => g.Key, g => g.Sum(x => x.Amount));

		var allMerchantIds = currentMerchantTotals.Keys
			.Where(id => currentMerchantTotals[id] > 0)
			.Concat(previousMerchantTotals.Keys)
			.Distinct()
			.ToList();

		var merchantLookup = await GetMerchantLookupAsync(allMerchantIds);
		var allMerchants = await merchantRepository.ListAsync(allMerchantIds);
		
		// Get all category IDs (from UserMerchant or Merchant)
		var categoryIds = new List<ObjectId>();
		foreach (var merchantId in allMerchantIds)
		{
			var userMerchant = merchantLookup.GetValueOrDefault(merchantId);
			if (userMerchant?.UserCategoryId != null)
			{
				categoryIds.Add(userMerchant.UserCategoryId.Value);
			}
			else
			{
				var merchant = allMerchants.FirstOrDefault(m => m.Id == merchantId);
				if (merchant != null)
				{
					categoryIds.Add(merchant.CategoryId);
				}
			}
		}
		var categoryLookup = await GetCategoryLookupAsync(categoryIds.Distinct().ToList());

		var currentTotal = currentMerchantTotals.Where(x => x.Value > 0).Sum(x => x.Value);
		var previousTotal = previousMerchantTotals.Values.Sum();

		var summary = BuildSummary(currentTotal, previousTotal);

		var merchantChanges = allMerchantIds
			.Where(id => currentMerchantTotals.GetValueOrDefault(id) > 0)
			.Select(id => BuildMerchantListItem(id, currentMerchantTotals, previousMerchantTotals, merchantLookup, allMerchants, categoryLookup))
			.OrderBy(x => GetMerchantDisplayName(x.MerchantId, x.MerchantName, merchantLookup, allMerchants))
			.ToList();

		// Top 9 merchants + "Other" for pie chart
		var top9Merchants = currentMerchantTotals
			.Where(x => x.Value > 0)
			.OrderByDescending(x => x.Value)
			.Take(Constants.Reports.TopMerchantsForDistribution)
			.ToList();

		var top9Total = top9Merchants.Sum(x => x.Value);
		var otherTotal = currentTotal - top9Total;

		var distribution = new List<ReportMerchantDistributionViewModel>();
		foreach (var kvp in top9Merchants)
		{
			var merchant = allMerchants.Single(x => x.Id == kvp.Key);
			var userMerchant = merchantLookup.GetValueOrDefault(kvp.Key);
			var displayName = !string.IsNullOrEmpty(userMerchant?.Nickname) ? userMerchant.Nickname : merchant.Name;

			distribution.Add(new ReportMerchantDistributionViewModel
			{
				MerchantId = kvp.Key.ToString(),
				MerchantName = displayName,
				CurrentMonthToDateTotal = kvp.Value,
				PercentageOfTotal = currentTotal > 0 ? Math.Round(kvp.Value / currentTotal * 100, 1) : 0
			});
		}

		if (otherTotal > 0)
		{
			distribution.Add(new ReportMerchantDistributionViewModel
			{
				MerchantId = "other",
				MerchantName = "Other",
				CurrentMonthToDateTotal = otherTotal,
				PercentageOfTotal = currentTotal > 0 ? Math.Round(otherTotal / currentTotal * 100, 1) : 0
			});
		}

		var response = new MerchantsReportOverviewResponseViewModel
		{
			Summary = summary,
			MerchantDistribution = distribution,
			Merchants = merchantChanges
		};

		return FunctionResponse.Success(response);
	}

	public async Task<FunctionResponse<MerchantDetailResponseViewModel>> GetMerchantDetailAsync(string merchantId, MerchantDetailRequestViewModel request)
	{
		var merchantObjectId = merchantId.ToObjectIdOrNull();
		if (merchantObjectId == null)
		{
			return FunctionResponse.Failure<MerchantDetailResponseViewModel>(MessageCodes.InvalidMerchantId);
		}

		if (!TryResolveTimezone(requestContextViewModel.Timezone, out var tz))
		{
			return FunctionResponse.Failure<MerchantDetailResponseViewModel>(MessageCodes.InvalidTimezone);
		}

		var nowLocal = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz).Date;
		var (startLocal, endLocal) = ResolveRange(request.StartDate, request.EndDate, nowLocal);
		var (startUtc, endUtc) = ToUtcRange(startLocal, endLocal, tz);

		var accountId = request.AccountId?.ToObjectIdOrNull();

		var userId = requestContextViewModel.UserId.ToObjectId();
		
		// Get transactions for this merchant
		var page = request.Page ?? 1;
		var pageSize = request.PageSize ?? Constants.Reports.DefaultPageSize;

		var (transactions, total) = await GetMerchantTransactionsAsync(
			userId,
			merchantObjectId.Value,
			accountId,
			startUtc,
			endUtc,
			page,
			pageSize);

		var totalAmount = transactions.Sum(x => x.Amount);

		var merchant = await merchantRepository.GetRequiredAsync(merchantObjectId.Value);
		var userMerchantFilter = Builders<UserMerchant>.Filter.And(
			Builders<UserMerchant>.Filter.Eq(x => x.UserId, userId),
			Builders<UserMerchant>.Filter.Eq(x => x.MerchantId, merchantObjectId.Value)
		);
		var userMerchant = (await userMerchantRepository.ListAsync(userMerchantFilter)).FirstOrDefault();
		var merchantName = !string.IsNullOrEmpty(userMerchant?.Nickname) ? userMerchant.Nickname : merchant.Name;

		var accountLookup = await GetAccountLookupAsync(transactions.Select(x => x.AccountId).Distinct().ToList());

		var items = transactions.Select(transaction => new TransactionItemViewModel
		{
			TransactionId = transaction.Id.ToString(),
			Date = transaction.DateTime.ToString("yyyy-MM-dd"),
			MerchantName = merchantName,
			AccountName = accountLookup[transaction.AccountId].NickName ?? accountLookup[transaction.AccountId].Name,
			Amount = transaction.Amount,
			TransactionName = transaction.TransactionName
		}).ToList();

		var totalPages = pageSize > 0 ? (int) Math.Ceiling((double) total / pageSize) : 1;

		// Build comparison if current month range
		MerchantComparisonViewModel? comparison = null;
		var isCurrentMonthRange = startLocal == new DateTime(nowLocal.Year, nowLocal.Month, 1) && endLocal == nowLocal.Date;
		if (isCurrentMonthRange)
		{
			var (prevStartLocal, prevEndLocal) = GetPreviousMonthSamePeriod(startLocal, endLocal);
			var (prevStartUtc, prevEndUtc) = ToUtcRange(prevStartLocal, prevEndLocal, tz);
			
			var (prevTransactions, _) = await GetMerchantTransactionsAsync(
				userId,
				merchantObjectId.Value,
				accountId,
				prevStartUtc,
				prevEndUtc,
				1,
				int.MaxValue);

			var previousTotal = prevTransactions.Sum(x => x.Amount);
			var comparisonSummary = BuildSummary(totalAmount, previousTotal);

			comparison = new MerchantComparisonViewModel
			{
				PreviousMonthSamePeriodTotal = comparisonSummary.PreviousMonthSamePeriodTotal,
				DifferenceAmount = comparisonSummary.DifferenceAmount,
				PercentageChange = comparisonSummary.PercentageChange,
				Trend = comparisonSummary.Trend,
				IsNewSpending = comparisonSummary.IsNewSpending
			};
		}

		var response = new MerchantDetailResponseViewModel
		{
			MerchantSummary = new MerchantSummaryViewModel
			{
				MerchantId = merchantObjectId.Value.ToString(),
				MerchantName = merchantName,
				StartDate = startLocal.ToString("yyyy-MM-dd"),
				EndDate = endLocal.ToString("yyyy-MM-dd"),
				TotalAmount = totalAmount,
				Comparison = comparison
			},
			Transactions = new TransactionListViewModel
			{
				Items = items,
				Total = (int) total,
				PageNumber = page,
				PageSize = pageSize,
				TotalPages = totalPages
			}
		};

		return FunctionResponse.Success(response);
	}

	private async Task<List<NormalizedTransaction>> GetMerchantTransactionTotalsAsync(ObjectId userId, DateTime startUtc, DateTime endUtc)
	{
		var filter = Builders<NormalizedTransaction>.Filter.And(
			Builders<NormalizedTransaction>.Filter.Eq(x => x.UserId, userId),
			Builders<NormalizedTransaction>.Filter.Gte(x => x.DateTime, startUtc),
			Builders<NormalizedTransaction>.Filter.Lte(x => x.DateTime, endUtc)
		);

		return await transactionRepository.ListAsync(filter);
	}

	private async Task<(List<NormalizedTransaction> Transactions, long Total)> GetMerchantTransactionsAsync(
		ObjectId userId,
		ObjectId merchantId,
		ObjectId? accountId,
		DateTime startUtc,
		DateTime endUtc,
		int page,
		int pageSize)
	{
		var filters = new List<FilterDefinition<NormalizedTransaction>>
		{
			Builders<NormalizedTransaction>.Filter.Eq(x => x.UserId, userId),
			Builders<NormalizedTransaction>.Filter.Eq(x => x.MerchantId, merchantId),
			Builders<NormalizedTransaction>.Filter.Gte(x => x.DateTime, startUtc),
			Builders<NormalizedTransaction>.Filter.Lte(x => x.DateTime, endUtc)
		};

		if (accountId.HasValue)
		{
			filters.Add(Builders<NormalizedTransaction>.Filter.Eq(x => x.AccountId, accountId.Value));
		}

		var filter = Builders<NormalizedTransaction>.Filter.And(filters);
		var total = await transactionRepository.CountAsync(filter);

		var allTransactions = await transactionRepository.ListAsync(filter);
		var sortedTransactions = allTransactions.OrderBy(x => x.TransactionName).ToList();
		
		var paging = new PagingParameter
		{
			PageNo = Math.Max(page - 1, 0),
			PageSize = pageSize
		};

		var transactions = sortedTransactions.Skip(paging.PageNo * paging.PageSize).Take(paging.PageSize).ToList();
		return (transactions, total);
	}

	private static ReportMerchantListItemViewModel BuildMerchantListItem(
		ObjectId merchantId,
		Dictionary<ObjectId, decimal> currentTotals,
		Dictionary<ObjectId, decimal> previousTotals,
		Dictionary<ObjectId, UserMerchant> userMerchantLookup,
		List<Merchant> allMerchants,
		Dictionary<ObjectId, UserCategory> categoryLookup)
	{
		currentTotals.TryGetValue(merchantId, out var current);
		previousTotals.TryGetValue(merchantId, out var previous);
		var difference = current - previous;
		var percentage = previous > 0 ? Math.Round(difference / previous * 100, 1) : 0;

		var merchant = allMerchants.Single(x => x.Id == merchantId);
		var userMerchant = userMerchantLookup.GetValueOrDefault(merchantId);
		var displayName = !string.IsNullOrEmpty(userMerchant?.Nickname) ? userMerchant.Nickname : merchant.Name;
		
		// Get category - prioritize UserMerchant's category, fallback to Merchant's category
		string categoryName = string.Empty;
		if (userMerchant?.UserCategoryId != null && categoryLookup.ContainsKey(userMerchant.UserCategoryId.Value))
		{
			categoryName = categoryLookup[userMerchant.UserCategoryId.Value].Name;
		}
		else if (categoryLookup.ContainsKey(merchant.CategoryId))
		{
			categoryName = categoryLookup[merchant.CategoryId].Name;
		}

		return new ReportMerchantListItemViewModel
		{
			MerchantId = merchantId.ToString(),
			MerchantName = displayName,
			CategoryName = categoryName,
			CurrentMonthToDateTotal = current,
			PreviousMonthSamePeriodTotal = previous,
			DifferenceAmount = difference,
			PercentageChange = percentage
		};
	}

	private static string GetMerchantDisplayName(
		string merchantId,
		string merchantName,
		Dictionary<ObjectId, UserMerchant> userMerchantLookup,
		List<Merchant> allMerchants)
	{
		var id = merchantId.ToObjectId();
		var userMerchant = userMerchantLookup.GetValueOrDefault(id);
		if (!string.IsNullOrEmpty(userMerchant?.Nickname))
		{
			return userMerchant.Nickname;
		}
		
		var merchant = allMerchants.FirstOrDefault(x => x.Id == id);
		return merchant?.Name ?? merchantName;
	}

	private async Task<AccountComparisonViewModel> BuildAccountComparison(DateTime startLocal,
		DateTime endLocal,
		DateTime nowLocal,
		ObjectId userId,
		ObjectId accountId,
		TimeZoneInfo tz,
		decimal currentTotal)
	{
		var isCurrentMonthRange = startLocal == new DateTime(nowLocal.Year, nowLocal.Month, 1) && endLocal == nowLocal.Date;
		if (!isCurrentMonthRange)
		{
			return new AccountComparisonViewModel
			{
				PreviousMonthSamePeriodTotal = 0,
				DifferenceAmount = 0,
				PercentageChange = 0,
				Trend = Constants.Reports.TrendNeutral,
				IsNewSpending = false
			};
		}

		var (prevStartLocal, prevEndLocal) = GetPreviousMonthSamePeriod(startLocal, endLocal);
		var (prevStartUtc, prevEndUtc) = ToUtcRange(prevStartLocal, prevEndLocal, tz);
		var previousSummaries = await GetDailySummariesAsync(userId,
			prevStartUtc,
			prevEndUtc,
			null,
			accountId);
		var previousTotal = previousSummaries.Sum(x => x.TotalAmount);

		var summary = BuildSummary(currentTotal, previousTotal);

		return new AccountComparisonViewModel
		{
			PreviousMonthSamePeriodTotal = summary.PreviousMonthSamePeriodTotal,
			DifferenceAmount = summary.DifferenceAmount,
			PercentageChange = summary.PercentageChange,
			Trend = summary.Trend,
			IsNewSpending = summary.IsNewSpending
		};
	}

	private async Task<List<DailyCategoryAccountExpense>> GetDailySummariesAsync(ObjectId userId,
		DateTime startUtc,
		DateTime endUtc,
		ObjectId? categoryId = null,
		ObjectId? accountId = null)
	{
		var filters = new List<FilterDefinition<DailyCategoryAccountExpense>>
		{
			Builders<DailyCategoryAccountExpense>.Filter.Eq(x => x.UserId, userId),
			Builders<DailyCategoryAccountExpense>.Filter.Gte(x => x.Date, startUtc.ToDateOnly()),
			Builders<DailyCategoryAccountExpense>.Filter.Lte(x => x.Date, endUtc.ToDateOnly())
		};

		if (categoryId.HasValue)
		{
			filters.Add(Builders<DailyCategoryAccountExpense>.Filter.Eq(x => x.CategoryId, categoryId.Value));
		}

		if (accountId.HasValue)
		{
			filters.Add(Builders<DailyCategoryAccountExpense>.Filter.Eq(x => x.AccountId, accountId.Value));
		}

		var filter = Builders<DailyCategoryAccountExpense>.Filter.And(filters);
		return await dailySummaryRepository.ListAsync(filter);
	}

	private async Task<(List<NormalizedTransaction> Transactions, long Total)> GetCategoryTransactionsAsync(ObjectId userId,
		ObjectId categoryId,
		ObjectId? accountId,
		DateTime startUtc,
		DateTime endUtc,
		int page,
		int pageSize)
	{
		var filter = Builders<NormalizedTransaction>.Filter.And(
			Builders<NormalizedTransaction>.Filter.Eq(x => x.UserId, userId),
			Builders<NormalizedTransaction>.Filter.Eq(x => x.UserCategoryId, categoryId),
			Builders<NormalizedTransaction>.Filter.Gte(x => x.DateTime, startUtc),
			Builders<NormalizedTransaction>.Filter.Lte(x => x.DateTime, endUtc)
		);

		if (accountId.HasValue)
		{
			filter = Builders<NormalizedTransaction>.Filter.And(filter, Builders<NormalizedTransaction>.Filter.Eq(x => x.AccountId, accountId.Value));
		}

		var total = await transactionRepository.CountAsync(filter);

		var sort = Builders<NormalizedTransaction>.Sort.Descending(x => x.DateTime);

		var paging = new PagingParameter
		{
			PageNo = Math.Max(page - 1, 0),
			PageSize = pageSize
		};

		var transactions = await transactionRepository.ListPagingAsync(
			x => x.UserId == userId && x.UserCategoryId == categoryId && x.DateTime >= startUtc && x.DateTime <= endUtc && (!accountId.HasValue || x.AccountId == accountId.Value),
			paging,
			sort);

		return (transactions, total);
	}

	private static ReportSummaryViewModel BuildSummary(decimal currentTotal, decimal previousTotal)
	{
		var difference = currentTotal - previousTotal;
		var isNew = previousTotal == 0 && currentTotal > 0;
		var percentage = previousTotal > 0 ? Math.Round(difference / previousTotal * 100, 1) : 0;
		var trend = difference > 0 ? Constants.Reports.TrendIncrease :
			difference < 0 ? Constants.Reports.TrendDecrease : Constants.Reports.TrendNeutral;

		return new ReportSummaryViewModel
		{
			CurrentMonthToDateTotal = currentTotal,
			PreviousMonthSamePeriodTotal = previousTotal,
			DifferenceAmount = difference,
			PercentageChange = percentage,
			Trend = trend,
			IsNewSpending = isNew
		};
	}

	private static ReportCategoryChangeViewModel BuildCategoryChange(ObjectId categoryId,
		Dictionary<ObjectId, decimal> currentTotals,
		Dictionary<ObjectId, decimal> previousTotals,
		Dictionary<ObjectId, UserCategory> categoryLookup)
	{
		currentTotals.TryGetValue(categoryId, out var current);
		previousTotals.TryGetValue(categoryId, out var previous);
		var difference = current - previous;
		var percentage = previous > 0 ? Math.Round(difference / previous * 100, 1) : 0;

		return new ReportCategoryChangeViewModel
		{
			CategoryId = categoryId.ToString(),
			CategoryName = categoryLookup[categoryId].Name,
			CurrentMonthToDateTotal = current,
			PreviousMonthSamePeriodTotal = previous,
			DifferenceAmount = difference,
			PercentageChange = percentage
		};
	}

	private static AccountListItemViewModel BuildAccountListItem(ObjectId accountId,
		Dictionary<ObjectId, decimal> currentTotals,
		Dictionary<ObjectId, decimal> previousTotals,
		Dictionary<ObjectId, Account> accountLookup)
	{
		currentTotals.TryGetValue(accountId, out var current);
		previousTotals.TryGetValue(accountId, out var previous);
		var difference = current - previous;
		var percentage = previous > 0 ? Math.Round(difference / previous * 100, 1) : 0;

		return new AccountListItemViewModel
		{
			AccountId = accountId.ToString(),
			AccountName = accountLookup[accountId].NickName ?? accountLookup[accountId].Name,
			CurrentMonthToDateTotal = current,
			PreviousMonthSamePeriodTotal = previous,
			DifferenceAmount = difference,
			PercentageChange = percentage
		};
	}

	private async Task<Dictionary<ObjectId, UserCategory>> GetCategoryLookupAsync(List<ObjectId> ids)
	{
		if (ids.Count == 0)
		{
			return new Dictionary<ObjectId, UserCategory>();
		}
		return await categoryRepository.ListDictionaryAsync(ids);
	}

	private async Task<Dictionary<ObjectId, Account>> GetAccountLookupAsync(List<ObjectId> ids)
	{
		if (ids.Count == 0)
		{
			return new Dictionary<ObjectId, Account>();
		}
		return await accountRepository.ListDictionaryAsync(ids);
	}

	private async Task<Dictionary<ObjectId, UserMerchant>> GetMerchantLookupAsync(List<ObjectId> ids)
	{
		if (ids.Count == 0)
		{
			return new Dictionary<ObjectId, UserMerchant>();
		}
		return await userMerchantRepository.ListSingleDictionaryAsync(ids, p => p.MerchantId);
	}

	private async Task<string> GetCategoryNameAsync(ObjectId categoryId)
	{
		var category = await categoryRepository.GetRequiredAsync(categoryId);
		return category.Name;
	}


	private static (DateTime StartLocal, DateTime EndLocal) ResolveRange(DateTime? start, DateTime? end, DateTime nowLocal)
	{
		var startLocal = (start ?? new DateTime(nowLocal.Year, nowLocal.Month, 1)).Date;
		var endLocal = (end ?? nowLocal.Date).Date;
		return (startLocal, endLocal);
	}

	private static (DateTime StartLocal, DateTime EndLocal) GetPreviousMonthSamePeriod(DateTime startLocal, DateTime endLocal)
	{
		var prevStart = startLocal.AddMonths(-1);
		var prevEnd = endLocal.AddMonths(-1);
		return (prevStart, prevEnd);
	}

	private static (DateTime StartUtc, DateTime EndUtc) ToUtcRange(DateTime startLocal, DateTime endLocal, TimeZoneInfo tz)
	{
		var startUnspecified = DateTime.SpecifyKind(startLocal.Date, DateTimeKind.Unspecified);
		var endUnspecified = DateTime.SpecifyKind(endLocal.Date.AddDays(1).AddTicks(-1), DateTimeKind.Unspecified);
		var startUtc = TimeZoneInfo.ConvertTimeToUtc(startUnspecified, tz);
		var endUtc = TimeZoneInfo.ConvertTimeToUtc(endUnspecified, tz);
		return (startUtc, endUtc);
	}

	private static bool TryResolveTimezone(string timezone, out TimeZoneInfo tz)
	{
		if (string.IsNullOrWhiteSpace(timezone))
		{
			tz = TimeZoneInfo.Utc;
			return false;
		}

		if (!TimeZoneInfo.TryFindSystemTimeZoneById(timezone, out var tzInfo))
		{
			tz = TimeZoneInfo.Utc;
			return false;
		}
		tz = tzInfo;
		return true;
	}
}
