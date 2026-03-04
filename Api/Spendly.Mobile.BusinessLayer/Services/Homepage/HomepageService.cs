// CHANGED_BY_AI: 2026-03-02 - Update homepage aggregation and response
/* AI-ALLOW: SPEC-11 - In-memory grouping per user request */
namespace Spendly.Mobile.BusinessLayer.Services.Homepage;

using MongoDB.Bson;
using MongoDB.Driver;
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
    IRepository<Category> categoryRepository,
    IRepository<Merchant> merchantRepository,
    RequestContextViewModel requestContextViewModel)
{
    [Cacheable(DurationSeconds=120)]
    public async virtual Task<FunctionResponse<HomepageResponseViewModel>> GetHomepageAsync()
    {
        var userId = requestContextViewModel.UserId.ToObjectId();
        var now = DateTime.UtcNow.Date;
        var currentMonthStart = new DateTime(now.Year, now.Month, 1);
        var previousMonthStart = currentMonthStart.AddMonths(-1);
        var previousMonthEnd = previousMonthStart.AddMonths(1).AddDays(-1);
        var previousMonthSameDayEnd = new DateTime(previousMonthStart.Year, previousMonthStart.Month, Math.Min(now.Day, DateTime.DaysInMonth(previousMonthStart.Year, previousMonthStart.Month)));

        var currentMonthSummaries = await GetDailySummariesAsync(userId, currentMonthStart, now);
        var previousMonthSummaries = await GetDailySummariesAsync(userId, previousMonthStart, previousMonthEnd);

        var currentMonthTotal = currentMonthSummaries.Sum(x => x.TotalAmount);
        var previousMonthTotal = previousMonthSummaries.Sum(x => x.TotalAmount);

        var previousMonthSameDayTotal = previousMonthSummaries
            .Where(x => x.Date.Date >= previousMonthStart && x.Date.Date <= previousMonthSameDayEnd)
            .Sum(x => x.TotalAmount);

        var midMonthComparison = BuildComparison(currentMonthTotal, previousMonthSameDayTotal);

        var accountIds = currentMonthSummaries.Select(x => x.AccountId)
            .Concat(previousMonthSummaries.Select(x => x.AccountId))
            .Distinct()
            .ToList();
        var categoryIds = currentMonthSummaries.Select(x => x.CategoryId)
            .Concat(previousMonthSummaries.Select(x => x.CategoryId))
            .Distinct()
            .ToList();

        var accounts = await accountRepository.ListAsync(
            Builders<Account>.Filter.In(x => x.Id, accountIds)
        );
        var categories = await categoryRepository.ListAsync(
            Builders<Category>.Filter.In(x => x.Id, categoryIds)
        );

        var accountLookup = accounts.ToDictionary(x => x.Id);
        var categoryLookup = categories.ToDictionary(x => x.Id);

        var spendingByAccountCurrent = BuildAccountDistribution(currentMonthSummaries, accountLookup, currentMonthTotal);
        var spendingByAccountPrevious = BuildAccountDistribution(previousMonthSummaries, accountLookup, previousMonthTotal);
        var spendingByCategoryCurrent = BuildCategoryDistribution(currentMonthSummaries, categoryLookup, currentMonthTotal);
        var spendingByCategoryPrevious = BuildCategoryDistribution(previousMonthSummaries, categoryLookup, previousMonthTotal);

        var sixMonthTrend = await GetSixMonthTrendAsync(userId, now);
        var latestExpenses = await GetLatestExpensesAsync(userId);
        var weeklySnapshot = await GetWeeklySnapshotAsync(userId, now);
        var topSpendingCategory = BuildTopCategory(currentMonthSummaries, categoryLookup, currentMonthTotal);
        var mostUsedAccount = BuildMostUsedAccount(currentMonthSummaries, accountLookup, currentMonthTotal);
        var highestSingleExpense = await GetHighestSingleExpenseAsync(userId, currentMonthStart, now);
        var dailyAverage = BuildDailyAverage(currentMonthTotal, previousMonthSameDayTotal, now, previousMonthStart);

        var response = new HomepageResponseViewModel
        {
            //Ust summary box
            CurrentMonthTotalSpending = currentMonthTotal,
            PreviousMonthTotalSpending = previousMonthTotal,
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

    private async Task<List<DailyCategoryAccountExpense>> GetDailySummariesAsync(ObjectId userId, DateTime start, DateTime end)
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

    private static List<SpendingByAccountViewModel> BuildAccountDistribution(
        List<DailyCategoryAccountExpense> summaries,
        Dictionary<ObjectId, Account> accounts,
        decimal total)
    {
        var grouped = summaries
            .GroupBy(x => x.AccountId)
            .Select(g => new { AccountId = g.Key, Amount = g.Sum(x => x.TotalAmount) })
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
                AccountName = account?.Name ?? "Unknown",
                Amount = g.Amount,
                PercentageOfTotal = total > 0 ? Math.Round(g.Amount / total * 100, 1) : 0
            };
        }).ToList();

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

    private static List<SpendingByCategoryViewModel> BuildCategoryDistribution(
        List<DailyCategoryAccountExpense> summaries,
        Dictionary<ObjectId, Category> categories,
        decimal total)
    {
        var grouped = summaries
            .GroupBy(x => x.CategoryId)
            .Select(g => new { CategoryId = g.Key, Amount = g.Sum(x => x.TotalAmount) })
            .OrderByDescending(x => x.Amount)
            .ToList();

        var top = grouped.Take(Constants.Homepage.CategoryTopCount).ToList();
        var remainder = grouped.Skip(Constants.Homepage.CategoryTopCount).Sum(x => x.Amount);

        var results = top.Select(g =>
        {
            categories.TryGetValue(g.CategoryId, out var category);
            return new SpendingByCategoryViewModel
            {
                CategoryId = g.CategoryId.ToString(),
                CategoryName = category?.Name ?? "Uncategorized",
                Amount = g.Amount,
                PercentageOfTotal = total > 0 ? Math.Round(g.Amount / total * 100, 1) : 0
            };
        }).ToList();

        if (remainder > 0)
        {
            results.Add(new SpendingByCategoryViewModel
            {
                CategoryId = string.Empty,
                CategoryName = "Other",
                Amount = remainder,
                PercentageOfTotal = total > 0 ? Math.Round(remainder / total * 100, 1) : 0
            });
        }

        return results;
    }

    private async Task<List<MonthlySpendingTrendViewModel>> GetSixMonthTrendAsync(ObjectId userId, DateTime now)
    {
        var start = new DateTime(now.Year, now.Month, 1).AddMonths(-(Constants.Homepage.TrendMonths - 1));
        var summaries = await GetDailySummariesAsync(userId, start, now);

        var totals = summaries
            .GroupBy(x => new { x.Date.Year, x.Date.Month })
            .ToDictionary(g => new DateTime(g.Key.Year, g.Key.Month, 1), g => g.Sum(x => x.TotalAmount));

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
        var sort = Builders<NormalizedTransaction>.Sort.Descending(x => x.Date);
        var transactions = await transactionRepository.ListAsync(p => p.UserId == userId, sort, 10);

        var accountIds = transactions.Select(t => t.AccountId).Distinct().ToList();
        var categoryIds = transactions.Where(t => t.CategoryId.HasValue).Select(t => t.CategoryId.Value).Distinct().ToList();
        var merchantIds = transactions.Select(t => t.MerchantId).Distinct().ToList();

        var accounts = await accountRepository.ListAsync(
            Builders<Account>.Filter.In(x => x.Id, accountIds)
        );

        var categories = await categoryRepository.ListAsync(
            Builders<Category>.Filter.In(x => x.Id, categoryIds)
        );

        var merchants = await merchantRepository.ListAsync(
            Builders<Merchant>.Filter.In(x => x.Id, merchantIds)
        );

        return transactions.Select(t =>
        {
            var account = accounts.FirstOrDefault(a => a.Id == t.AccountId);
            var category = t.CategoryId.HasValue ? categories.FirstOrDefault(c => c.Id == t.CategoryId.Value) : null;
            var merchant = merchants.FirstOrDefault(m => m.Id == t.MerchantId);

            return new LatestExpenseViewModel
            {
                TransactionId = t.Id.ToString(),
                Date = t.Date,
                Amount = t.Amount,
                CategoryName = category?.Name ?? "Uncategorized",
                MerchantName = merchant?.Name ?? "Unknown",
                AccountName = account?.Name ?? "Unknown"
            };
        }).ToList();
    }

    private async Task<WeeklySnapshotViewModel> GetWeeklySnapshotAsync(ObjectId userId, DateTime now)
    {
        var weekStart = GetWeekStart(now);
        var dayCount = (now - weekStart).Days;
        var weekEnd = now;
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

    private static DateTime GetWeekStart(DateTime date)
    {
        var diff = (7 + (int)date.DayOfWeek - (int)DayOfWeek.Monday) % 7;
        return date.AddDays(-diff).Date;
    }

    private static TopSpendingCategoryViewModel BuildTopCategory(
        List<DailyCategoryAccountExpense> summaries,
        Dictionary<ObjectId, Category> categories,
        decimal total)
    {
        var top = summaries
            .GroupBy(x => x.CategoryId)
            .Select(g => new { CategoryId = g.Key, Amount = g.Sum(x => x.TotalAmount) })
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

        categories.TryGetValue(top.CategoryId, out var category);

        return new TopSpendingCategoryViewModel
        {
            CategoryName = category?.Name ?? "Uncategorized",
            Amount = top.Amount,
            PercentageOfTotal = total > 0 ? Math.Round(top.Amount / total * 100, 1) : 0
        };
    }

    private static MostUsedAccountViewModel BuildMostUsedAccount(
        List<DailyCategoryAccountExpense> summaries,
        Dictionary<ObjectId, Account> accounts,
        decimal total)
    {
        var top = summaries
            .GroupBy(x => x.AccountId)
            .Select(g => new { AccountId = g.Key, Amount = g.Sum(x => x.TotalAmount) })
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
            AccountName = account?.Name ?? "Unknown",
            PercentageShare = total > 0 ? Math.Round(top.Amount / total * 100, 1) : 0
        };
    }

    private async Task<HighestSingleExpenseViewModel> GetHighestSingleExpenseAsync(ObjectId userId, DateTime start, DateTime end)
    {
        var sort = Builders<NormalizedTransaction>.Sort.Descending(x => x.Amount).Descending(x => x.Date);
        var transaction = (await transactionRepository.ListAsync(p=>p.UserId == userId&&p.Date>=start &&p.Date<=end, sort, 1)).FirstOrDefault();

        if (transaction == null)
        {
            return new HighestSingleExpenseViewModel
            {
                MerchantName = string.Empty,
                Amount = 0,
                Date = start
            };
        }

        var merchant = await merchantRepository.GetAsync(transaction.MerchantId);

        return new HighestSingleExpenseViewModel
        {
            MerchantName = merchant?.Name ?? "Unknown",
            Amount = transaction.Amount,
            Date = transaction.Date
        };
    }

    private static DailyAverageViewModel BuildDailyAverage(decimal currentTotal, decimal previousTotal, DateTime now, DateTime previousMonthStart)
    {
        var currentDays = now.Day;
        var previousDays = Math.Min(now.Day, DateTime.DaysInMonth(previousMonthStart.Year, previousMonthStart.Month));

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
