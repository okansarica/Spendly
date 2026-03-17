// CHANGED_BY_AI: 2026-03-02 - Update homepage aggregation and response
/* AI-ALLOW: SPEC-11 - In-memory grouping per user request */
namespace Spendly.Mobile.BusinessLayer.Services.Homepage;

using MongoDB.Bson;
using MongoDB.Driver;
using Shared.Core.Extensions;
using Shared.Entities.Reporting;
using Shared.Entities.TransactionManagement;
using Shared.Entities.UserManagement;
using Spendly.Mobile.BusinessLayer.Constants;
using Spendly.Mobile.ViewModels.Homepage;
using Spendly.Shared.Core;
using Spendly.Shared.Core.Interception;
using Spendly.Shared.DataLayer;
using Spendly.Shared.ViewModels;

public class HomepageService(
	IRepository<DailyCategoryAccountExpense> dailySummaryRepository,
	IRepository<NormalizedTransaction> transactionRepository,
	IRepository<Account> accountRepository,
	IRepository<UserCategory> userCategoryRepository,
	IRepository<UserMerchant> userMerchantRepository,
	IRepository<Merchant> merchantRepository,
	RequestContextViewModel requestContextViewModel)
{
	//[Cacheable(DurationSeconds = 120)]
	public async virtual Task<FunctionResponse<HomepageResponseViewModel>> GetHomepageAsync()
	{
		var userId = requestContextViewModel.UserId.ToObjectId();
		var today = DateTime.UtcNow.ToDateOnly();
		var currentMonthStart = new DateTime(today.Year, today.Month, 1).ToDateOnly();
		var previousMonthStart = currentMonthStart.AddMonths(-1);
		var previousMonthEnd = previousMonthStart.AddMonths(1).AddDays(-1);
		var previousMonthSameDayEnd = new DateTime(previousMonthStart.Year, previousMonthStart.Month, Math.Min(today.Day, DateTime.DaysInMonth(previousMonthStart.Year, previousMonthStart.Month))).ToDateOnly();

		var currentMonthSummaries = await GetDailySummariesAsync(userId, currentMonthStart, today);
		var previousMonthSummaries = await GetDailySummariesAsync(userId, previousMonthStart, previousMonthEnd);

		var currentMonthTotal = currentMonthSummaries.Sum(x => x.TotalAmount);
		var previousMonthTotal = previousMonthSummaries.Sum(x => x.TotalAmount);
		var previousMonthSamePeriodTotal = previousMonthSummaries.Where(p=>p.Date<previousMonthSameDayEnd.AddDays(1)).Sum(x => x.TotalAmount);

		var previousMonthSameDayTotal = previousMonthSummaries
			.Where(x => x.Date >= previousMonthStart && x.Date <= previousMonthSameDayEnd)
			.Sum(x => x.TotalAmount);

		var midMonthComparison = BuildComparison(currentMonthTotal, previousMonthSamePeriodTotal);

		var accountIds = currentMonthSummaries.Select(x => x.AccountId)
			.Concat(previousMonthSummaries.Select(x => x.AccountId))
			.Distinct()
			.ToList();
		var userCategoryIds = currentMonthSummaries.Select(x => x.UserCategoryId)
			.Concat(previousMonthSummaries.Select(x => x.UserCategoryId))
			.Distinct()
			.ToList();

		var accounts = await accountRepository.ListAsync(
			Builders<Account>.Filter.In(x => x.Id, accountIds)
		);
		var categories = await userCategoryRepository.ListAsync(
			Builders<UserCategory>.Filter.In(x => x.Id, userCategoryIds)
		);

		var accountLookup = accounts.ToDictionary(x => x.Id);
		var categoryLookup = categories.ToDictionary(x => x.Id);

		var spendingByAccountCurrent = BuildAccountDistribution(currentMonthSummaries, accountLookup, currentMonthTotal);
		var spendingByAccountPrevious = BuildAccountDistribution(previousMonthSummaries, accountLookup, previousMonthTotal);
		var spendingByCategoryCurrent = BuildCategoryDistribution(currentMonthSummaries, categoryLookup, currentMonthTotal);
		var spendingByCategoryPrevious = BuildCategoryDistribution(previousMonthSummaries, categoryLookup, previousMonthTotal);

		var sixMonthTrend = await GetSixMonthTrendAsync(userId, today);
		var latestExpenses = await GetLatestExpensesAsync(userId);
		var weeklySnapshot = await GetWeeklySnapshotAsync(userId, today);
		var topSpendingCategory = BuildTopCategory(currentMonthSummaries, categoryLookup, currentMonthTotal);
		var mostUsedAccount = BuildMostUsedAccount(currentMonthSummaries, accountLookup, currentMonthTotal);
		var highestSingleExpense = await GetHighestSingleExpenseAsync(userId, currentMonthStart, today);
		var dailyAverage = BuildDailyAverage(currentMonthTotal,
			previousMonthSameDayTotal,
			today,
			previousMonthStart);

		var response = new HomepageResponseViewModel
		{
			//Ust summary box
			CurrentMonthTotalSpending = currentMonthTotal,
			PreviousMonthTotalSpending = previousMonthTotal,
			PreviousMonthSamePeriodTotalSpending = previousMonthSamePeriodTotal,
			MidMonthComparison = midMonthComparison,

			//Kucuk summary boxlar
			WeeklySnapshot = weeklySnapshot,
			DailyAverage = dailyAverage,

			//Insights
			TopSpendingCategory = topSpendingCategory,
			HighestSingleExpense = highestSingleExpense,
			MostUsedAccount = mostUsedAccount,

			//Pie charts
			SpendingByAccountCurrentMonth = spendingByAccountCurrent,
			SpendingByAccountPreviousMonth = spendingByAccountPrevious,
			SpendingByCategoryCurrentMonth = spendingByCategoryCurrent,
			SpendingByCategoryPreviousMonth = spendingByCategoryPrevious,

			//Bar chart
			SixMonthTrend = sixMonthTrend, //bar chart
			LatestExpenses = latestExpenses,
		};

		return FunctionResponse.Success(response);
	}

	private async Task<List<DailyCategoryAccountExpense>> GetDailySummariesAsync(ObjectId userId, DateOnly start, DateOnly end)
	{
		var filter = Builders<DailyCategoryAccountExpense>.Filter.And(
			Builders<DailyCategoryAccountExpense>.Filter.Eq(x => x.UserId, userId),
			Builders<DailyCategoryAccountExpense>.Filter.Gte(x => x.Date, start),
			Builders<DailyCategoryAccountExpense>.Filter.Lte(x => x.Date, end)
		);

		return await dailySummaryRepository.ListAsync(filter);
	}

	private static MidMonthComparisonViewModel BuildComparison(decimal current, decimal previous)
	{
		var percentage = CalculatePercentageChange(current, previous);
		return new MidMonthComparisonViewModel
		{
			IsIncreased = current > previous,
			PercentageChange = percentage
		};
	}

	private static decimal CalculatePercentageChange(decimal current, decimal previous)
	{
		if (previous <= 0)
		{
			return 0;
		}

		return Math.Round((current - previous) / previous * 100, 1);
	}

	private static List<SpendingByAccountViewModel> BuildAccountDistribution(List<DailyCategoryAccountExpense> summaries,
		Dictionary<ObjectId, Account> accounts,
		decimal total)
	{
		var grouped = summaries
			.GroupBy(x => x.AccountId)
			.Select(g => new {AccountId = g.Key, Amount = g.Sum(x => x.TotalAmount)})
			.OrderByDescending(x => x.Amount)
			.ToList();

		var top = grouped.Take(Constants.Homepage.AccountTopCount).ToList();
		var remainder = grouped.Skip(Constants.Homepage.AccountTopCount).Sum(x => x.Amount);

		var results = top.Select(g =>
			{
				accounts.TryGetValue(g.AccountId, out var account);
				return new SpendingByAccountViewModel
				{
					AccountId = g.AccountId.ToString(),
					AccountName = account!.NickName ?? account.Name,
					Amount = g.Amount,
					PercentageOfTotal = total > 0 ? Math.Round(g.Amount / total * 100, 1) : 0
				};
			})
			.ToList();

		if (remainder > 0)
		{
			results.Add(new SpendingByAccountViewModel
			{
				AccountId = string.Empty,
				AccountName = "Other",
				Amount = remainder,
				PercentageOfTotal = total > 0 ? Math.Round(remainder / total * 100, 1) : 0
			});
		}

		return results;
	}

	private static List<SpendingByCategoryViewModel> BuildCategoryDistribution(List<DailyCategoryAccountExpense> summaries,
		Dictionary<ObjectId, UserCategory> categories,
		decimal total)
	{
		var grouped = summaries
			.GroupBy(x => x.UserCategoryId)
			.Select(g => new {CategoryId = g.Key, Amount = g.Sum(x => x.TotalAmount)})
			.OrderByDescending(x => x.Amount)
			.ToList();

		var results = grouped.Select(g =>
			{
				categories.TryGetValue(g.CategoryId, out var category);
				return new SpendingByCategoryViewModel
				{
					CategoryId = g.CategoryId.ToString(),
					CategoryName = category!.Name,
					Amount = g.Amount,
					PercentageOfTotal = total > 0 ? Math.Round(g.Amount / total * 100, 1) : 0
				};
			})
			.ToList();

		return results;
	}

	private async Task<List<MonthlySpendingTrendViewModel>> GetSixMonthTrendAsync(ObjectId userId, DateOnly now)
	{
		var start = new DateTime(now.Year, now.Month, 1).AddMonths(-(Constants.Homepage.TrendMonths - 1)).ToDateOnly();
		var summaries = await GetDailySummariesAsync(userId, start, now);

		var totals = summaries
			.GroupBy(x => new {x.Date.Year, x.Date.Month})
			.ToDictionary(g => new DateTime(g.Key.Year, g.Key.Month, 1).ToDateOnly(), g => g.Sum(x => x.TotalAmount));

		var results = new List<MonthlySpendingTrendViewModel>();

		for (var i = 0; i < Constants.Homepage.TrendMonths; i++)
		{
			var month = start.AddMonths(i);
			totals.TryGetValue(month, out var currentTotal);
			var previousMonth = month.AddMonths(-1);
			totals.TryGetValue(previousMonth, out var previousTotal);

			results.Add(new MonthlySpendingTrendViewModel
			{
				Year = month.Year,
				Month = month.Month,
				Amount = currentTotal,
				PreviousMonthAmount = previousTotal,
				PercentageChange = CalculatePercentageChange(currentTotal, previousTotal)
			});
		}

		return results;
	}

	private async Task<List<LatestExpenseViewModel>> GetLatestExpensesAsync(ObjectId userId)
	{
		var sort = Builders<NormalizedTransaction>.Sort.Descending(x => x.DateTime);
		var normalizedTransactions = await transactionRepository.ListAsync(p => p.UserId == userId, sort, 10);

		var accountIds = normalizedTransactions.Select(t => t.AccountId).Distinct().ToList();
		var userCategoryIds = normalizedTransactions.Select(t => t.UserCategoryId).Distinct().ToList();
		var merchantIds = normalizedTransactions.Select(t => t.MerchantId).Distinct().ToList();

		var accounts = await accountRepository.ListAsync(
			Builders<Account>.Filter.In(x => x.Id, accountIds)
		);

		var userCategories = await userCategoryRepository.ListAsync(
			Builders<UserCategory>.Filter.In(x => x.Id, userCategoryIds)
		);

		var allMerchantdIds = normalizedTransactions.Select(p => p.MerchantId).Distinct().ToList();

		var allMerchants = await merchantRepository.ListDictionaryAsync(allMerchantdIds);

		var allUserMerchants = await userMerchantRepository.ListAsync(p=>p.UserId == userId && allMerchantdIds.Contains(p.MerchantId));

		var result = new List<LatestExpenseViewModel>();

		foreach (var transaction in normalizedTransactions)
		{
			var account = accounts.FirstOrDefault(a => a.Id == transaction.AccountId);
			var userCategory = userCategories.FirstOrDefault(c => c.Id == transaction.UserCategoryId);

			string? merchantName;

			var userMerchant = allUserMerchants.Single(p=>p.MerchantId == transaction.MerchantId);
			if (string.IsNullOrEmpty(userMerchant.Nickname))
			{
				merchantName = allMerchants[transaction.MerchantId].Name;
			}
			else
			{
				merchantName = userMerchant.Nickname;
			}

			result.Add(new LatestExpenseViewModel
			{
				TransactionId = transaction.Id.ToString(),
				Date = transaction.DateTime,
				Amount = transaction.Amount,
				CategoryName = userCategory?.Name ?? "",
				MerchantName = merchantName,
				AccountName = account!.NickName ?? account.Name,
				TransactionName = transaction.TransactionName
			});
		}

		return result;
	}

	private async Task<WeeklySnapshotViewModel> GetWeeklySnapshotAsync(ObjectId userId, DateOnly today)
	{
		var weekStart = GetWeekStart(today);
		var dayCount = today.DayNumber - weekStart.DayNumber;
		var weekEnd = today;
		var previousWeekStart = weekStart.AddDays(-7);
		var previousWeekEnd = previousWeekStart.AddDays(dayCount);

		var thisWeekSummaries = await GetDailySummariesAsync(userId, weekStart, weekEnd);
		var previousWeekSummaries = await GetDailySummariesAsync(userId, previousWeekStart, previousWeekEnd);

		var thisWeekTotal = thisWeekSummaries.Sum(x => x.TotalAmount);
		var previousWeekTotal = previousWeekSummaries.Sum(x => x.TotalAmount);

		return new WeeklySnapshotViewModel
		{
			ThisWeekTotal = thisWeekTotal,
			PreviousWeekTotal = previousWeekTotal,
			PercentageChange = CalculatePercentageChange(thisWeekTotal, previousWeekTotal),
			IsIncreased = thisWeekTotal > previousWeekTotal
		};
	}

	private static DateOnly GetWeekStart(DateOnly date)
	{
		var diff = (7 + (int) date.DayOfWeek - (int) DayOfWeek.Monday) % 7;
		return date.AddDays(-diff);
	}

	private static TopSpendingCategoryViewModel BuildTopCategory(List<DailyCategoryAccountExpense> summaries,
		Dictionary<ObjectId, UserCategory> categories,
		decimal total)
	{
		var top = summaries
			.GroupBy(x => x.UserCategoryId)
			.Select(g => new {CategoryId = g.Key, Amount = g.Sum(x => x.TotalAmount)})
			.OrderByDescending(x => x.Amount)
			.FirstOrDefault();

		if (top == null)
		{
			return new TopSpendingCategoryViewModel
			{
				CategoryName = string.Empty,
				Amount = 0,
				PercentageOfTotal = 0
			};
		}

		return new TopSpendingCategoryViewModel
		{
			CategoryName = categories[top.CategoryId].Name,
			Amount = top.Amount,
			PercentageOfTotal = total > 0 ? Math.Round(top.Amount / total * 100, 1) : 0
		};
	}

	private static MostUsedAccountViewModel BuildMostUsedAccount(List<DailyCategoryAccountExpense> summaries,
		Dictionary<ObjectId, Account> accounts,
		decimal total)
	{
		var top = summaries
			.GroupBy(x => x.AccountId)
			.Select(g => new {AccountId = g.Key, Amount = g.Sum(x => x.TotalAmount)})
			.OrderByDescending(x => x.Amount)
			.FirstOrDefault();

		if (top == null)
		{
			return new MostUsedAccountViewModel
			{
				AccountName = string.Empty,
				PercentageShare = 0
			};
		}

		accounts.TryGetValue(top.AccountId, out var account);

		return new MostUsedAccountViewModel
		{
			AccountName = account!.NickName ?? account.Name,
			PercentageShare = total > 0 ? Math.Round(top.Amount / total * 100, 1) : 0
		};
	}

	private async Task<HighestSingleExpenseViewModel> GetHighestSingleExpenseAsync(ObjectId userId, DateOnly start, DateOnly end)
	{
		//TODO buna cozum dusun tehlikeli !!!
		var sort = Builders<NormalizedTransaction>.Sort.Descending(x => x.Amount).Descending(x => x.DateTime);
		var transaction = (await transactionRepository.ListAsync(p => p.UserId == userId && p.DateTime >= start.ToDateTime() && p.DateTime <= end.ToDateTime(), sort, 1)).FirstOrDefault();

		if (transaction == null)
		{
			return new HighestSingleExpenseViewModel
			{
				MerchantName = string.Empty,
				Amount = 0,
				DateTime = start.ToDateTime(),
			};
		}

		string? merchantName;

		var userMerchant = await userMerchantRepository.GetRequiredAsync(p => p.MerchantId == transaction.MerchantId);
		if (string.IsNullOrEmpty(userMerchant.Nickname))
		{
			var merchant = await merchantRepository.GetRequiredAsync(transaction.MerchantId);
			merchantName = merchant.Name;
		}
		else
		{
			merchantName = userMerchant.Nickname;
		}


		return new HighestSingleExpenseViewModel
		{
			TransactionName =transaction.TransactionName,
			MerchantName = merchantName,
			Amount = transaction.Amount,
			DateTime = transaction.DateTime
		};
	}

	private static DailyAverageViewModel BuildDailyAverage(decimal currentTotal,
		decimal previousTotal,
		DateOnly today,
		DateOnly previousMonthStart)
	{
		var currentDays = today.Day;
		var previousDays = Math.Min(today.Day, DateTime.DaysInMonth(previousMonthStart.Year, previousMonthStart.Month));

		var currentAverage = currentDays > 0 ? currentTotal / currentDays : 0;
		var previousAverage = previousDays > 0 ? previousTotal / previousDays : 0;

		return new DailyAverageViewModel
		{
			CurrentMonthAverage = currentAverage,
			PreviousMonthAverage = previousAverage,
			PercentageChange = CalculatePercentageChange(currentAverage, previousAverage),
			IsIncreased = currentAverage > previousAverage
		};
	}
}
