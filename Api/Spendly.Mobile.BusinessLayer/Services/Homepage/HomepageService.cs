namespace Spendly.Mobile.BusinessLayer.Services.Homepage;

using MongoDB.Bson;
using MongoDB.Driver;
using Shared.Entities.Reporting;
using Shared.Entities.TransactionManagement;
using Shared.Entities.UserManagement;
using Spendly.Mobile.ViewModels.Homepage;
using Spendly.Shared.Core.Interception;
using Spendly.Shared.DataLayer;
using Spendly.Shared.ViewModels;

public class HomepageService(
    IRepository<DailyCategoryAccountExpense> dailySummaryRepository,
    IRepository<NormalizedTransaction> transactionRepository,
    IRepository<Account> accountRepository,
    IRepository<Category> categoryRepository,
    IRepository<Merchant> merchantRepository)
{
    [Cacheable(DurationSeconds=120)]
    public async virtual Task<FunctionResponse<HomepageResponseViewModel>> GetHomepageAsync(ObjectId userId)
    {
        var now = DateTime.UtcNow;
        var previousMonth = now.AddMonths(-1);
        var previousMonthStart = new DateTime(previousMonth.Year, previousMonth.Month, 1);
        var previousMonthEnd = previousMonthStart.AddMonths(1).AddDays(-1);
        
        var sixMonthsAgo = now.AddMonths(-6);
        var sixMonthsAgoStart = new DateTime(sixMonthsAgo.Year, sixMonthsAgo.Month, 1);

        var previousMonthTotal = await CalculatePreviousMonthTotalAsync(userId, previousMonthStart, previousMonthEnd);
        var spendingByAccount = await GetSpendingByAccountAsync(userId, previousMonthStart, previousMonthEnd);
        var spendingByCategory = await GetSpendingByCategoryAsync(userId, previousMonthStart, previousMonthEnd);
        var sixMonthTrend = await GetSixMonthTrendAsync(userId, sixMonthsAgoStart);
        var latestExpenses = await GetLatestExpensesAsync(userId);

        var response = new HomepageResponseViewModel
        {
            PreviousMonthTotalSpending = previousMonthTotal,
            SpendingByAccount = spendingByAccount,
            SpendingByCategory = spendingByCategory,
            SixMonthTrend = sixMonthTrend,
            LatestExpenses = latestExpenses
        };

        return FunctionResponse.Success(response);
    }

    private async Task<decimal> CalculatePreviousMonthTotalAsync(ObjectId userId, DateTime start, DateTime end)
    {
        var filter = Builders<DailyCategoryAccountExpense>.Filter.And(
            Builders<DailyCategoryAccountExpense>.Filter.Eq(x => x.UserId, userId),
            Builders<DailyCategoryAccountExpense>.Filter.Gte(x => x.Date, start),
            Builders<DailyCategoryAccountExpense>.Filter.Lte(x => x.Date, end)
        );

        var summaries = await dailySummaryRepository.ListAsync(filter);
        return summaries.Sum(x => x.TotalAmount);
    }

    private async Task<List<SpendingByAccountViewModel>> GetSpendingByAccountAsync(ObjectId userId, DateTime start, DateTime end)
    {
        var filter = Builders<DailyCategoryAccountExpense>.Filter.And(
            Builders<DailyCategoryAccountExpense>.Filter.Eq(x => x.UserId, userId),
            Builders<DailyCategoryAccountExpense>.Filter.Gte(x => x.Date, start),
            Builders<DailyCategoryAccountExpense>.Filter.Lte(x => x.Date, end)
        );

        var summaries = await dailySummaryRepository.ListAsync(filter);
        
        var groupedByAccount = summaries
            .GroupBy(x => x.AccountId)
            .Select(g => new { AccountId = g.Key, Amount = g.Sum(x => x.TotalAmount) })
            .ToList();

        var accounts = await accountRepository.ListAsync(
            Builders<Account>.Filter.In(x => x.Id, groupedByAccount.Select(g => g.AccountId))
        );

        return groupedByAccount.Select(g =>
        {
            var account = accounts.FirstOrDefault(a => a.Id == g.AccountId);
            return new SpendingByAccountViewModel
            {
                AccountId = g.AccountId.ToString(),
                AccountName = account?.Name ?? "Unknown",
                Amount = g.Amount
            };
        }).ToList();
    }

    private async Task<List<SpendingByCategoryViewModel>> GetSpendingByCategoryAsync(ObjectId userId, DateTime start, DateTime end)
    {
        var filter = Builders<DailyCategoryAccountExpense>.Filter.And(
            Builders<DailyCategoryAccountExpense>.Filter.Eq(x => x.UserId, userId),
            Builders<DailyCategoryAccountExpense>.Filter.Gte(x => x.Date, start),
            Builders<DailyCategoryAccountExpense>.Filter.Lte(x => x.Date, end)
        );

        var summaries = await dailySummaryRepository.ListAsync(filter);
        
        var groupedByCategory = summaries
            .GroupBy(x => x.CategoryId)
            .Select(g => new { CategoryId = g.Key, Amount = g.Sum(x => x.TotalAmount) })
            .ToList();

        var categories = await categoryRepository.ListAsync(
            Builders<Category>.Filter.In(x => x.Id, groupedByCategory.Select(g => g.CategoryId))
        );

        return groupedByCategory.Select(g =>
        {
            var category = categories.FirstOrDefault(c => c.Id == g.CategoryId);
            return new SpendingByCategoryViewModel
            {
                CategoryId = g.CategoryId.ToString(),
                CategoryName = category?.Name ?? "Unknown",
                Amount = g.Amount
            };
        }).ToList();
    }

    private async Task<List<MonthlySpendingTrendViewModel>> GetSixMonthTrendAsync(ObjectId userId, DateTime startDate)
    {
        var filter = Builders<DailyCategoryAccountExpense>.Filter.And(
            Builders<DailyCategoryAccountExpense>.Filter.Eq(x => x.UserId, userId),
            Builders<DailyCategoryAccountExpense>.Filter.Gte(x => x.Date, startDate)
        );

        var summaries = await dailySummaryRepository.ListAsync(filter);
        
        return summaries
            .GroupBy(x => new { x.Date.Year, x.Date.Month })
            .Select(g => new MonthlySpendingTrendViewModel
            {
                Year = g.Key.Year,
                Month = g.Key.Month,
                Amount = g.Sum(x => x.TotalAmount)
            })
            .OrderBy(x => x.Year)
            .ThenBy(x => x.Month)
            .ToList();
    }

    private async Task<List<LatestExpenseViewModel>> GetLatestExpensesAsync(ObjectId userId)
    {
        var sort = Builders<NormalizedTransaction>.Sort.Descending(x => x.Date);
        
        var transactions = await transactionRepository.ListAsync(p=>p.UserId == userId, sort, 10);

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
}

