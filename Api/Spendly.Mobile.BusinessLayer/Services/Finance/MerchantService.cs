// CHANGED_BY_AI: 2026-03-02 - Add merchant service
namespace Spendly.Mobile.BusinessLayer.Services.Finance;

using MongoDB.Bson;
using Spendly.Mobile.BusinessLayer.Constants;
using Spendly.Mobile.ViewModels.Finance;
using Spendly.Shared.DataLayer;
using Spendly.Shared.Entities.TransactionManagement;
using Spendly.Shared.Entities.UserManagement;
using Spendly.Shared.Localization;
using Spendly.Shared.ViewModels;

public class MerchantService(
    IRepository<Merchant> merchantRepository,
    IRepository<Category> categoryRepository,
    IRepository<NormalizedTransaction> transactionRepository)
{
    public async Task<FunctionResponse<List<MerchantListItemViewModel>>> GetListAsync(ObjectId userId, MerchantListRequestViewModel request)
    {
        var merchants = (await merchantRepository.ListAsync(x => x.UserId == userId)).ToList();

        if (request.IsUncategorized == true)
        {
            merchants = merchants.Where(x => x.CategoryId == null).ToList();
        }

        var search = request.Search?.Trim();
        if (!string.IsNullOrWhiteSpace(search))
        {
            merchants = merchants
                .Where(x => x.Name.Contains(search, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        if (merchants.Count == 0)
        {
            return FunctionResponse.Success(new List<MerchantListItemViewModel>());
        }

        var categoryLookup = (await categoryRepository.ListAsync(x => x.UserId == userId || x.IsSystem))
            .ToDictionary(x => x.Id, x => x.Name);

        var stats = await GetTransactionStatsAsync(userId, merchants.Select(x => x.Id).ToList(), request.StartDate, request.EndDate);

        var items = merchants.Select(x => new MerchantListItemViewModel
        {
            Id = x.Id.ToString(),
            Name = x.Name,
            CategoryId = x.CategoryId?.ToString(),
            CategoryName = x.CategoryId != null && categoryLookup.TryGetValue(x.CategoryId.Value, out var name) ? name : "Uncategorized",
            TransactionCount = stats.TryGetValue(x.Id, out var stat) ? stat.Count : 0,
            TotalAmount = stats.TryGetValue(x.Id, out var stat2) ? stat2.TotalAmount : 0,
            LastTransactionDate = stats.TryGetValue(x.Id, out var stat3) ? stat3.LastDate : null
        }).ToList();

        var sorted = ApplyMerchantSort(items, request.SortBy, request.SortDirection);
        return FunctionResponse.Success(sorted);
    }

    public async Task<FunctionResponse<MerchantDetailViewModel>> GetDetailAsync(ObjectId userId, string id, MerchantListRequestViewModel request)
    {
        if (!ObjectId.TryParse(id, out var merchantId))
        {
            return FunctionResponse.Failure<MerchantDetailViewModel>(MessageCodes.MerchantNotFound);
        }

        var merchant = await merchantRepository.GetAsync(x => x.Id == merchantId && x.UserId == userId);
        if (merchant == null)
        {
            return FunctionResponse.Failure<MerchantDetailViewModel>(MessageCodes.MerchantNotFound);
        }

        var categoryName = string.Empty;
        if (merchant.CategoryId != null)
        {
            var category = await categoryRepository.GetAsync(x => x.Id == merchant.CategoryId && (x.UserId == userId || x.IsSystem));
            categoryName = category?.Name ?? string.Empty;
        }

        var stats = await GetTransactionStatsAsync(userId, new List<ObjectId> { merchant.Id }, request.StartDate, request.EndDate);
        var total = stats.TryGetValue(merchant.Id, out var stat) ? stat.TotalAmount : 0;
        var count = stats.TryGetValue(merchant.Id, out var stat2) ? stat2.Count : 0;

        var response = new MerchantDetailViewModel
        {
            Id = merchant.Id.ToString(),
            Name = merchant.Name,
            CategoryId = merchant.CategoryId?.ToString(),
            CategoryName = string.IsNullOrWhiteSpace(categoryName) ? "Uncategorized" : categoryName,
            TransactionCount = count,
            TotalAmount = total
        };

        return FunctionResponse.Success(response);
    }

    public async Task<FunctionResponse<MerchantDetailViewModel>> UpdateCategoryAsync(ObjectId userId, string id, MerchantCategoryUpdateRequestViewModel request)
    {
        if (!ObjectId.TryParse(id, out var merchantId))
        {
            return FunctionResponse.Failure<MerchantDetailViewModel>(MessageCodes.MerchantNotFound);
        }

        var merchant = await merchantRepository.GetAsync(x => x.Id == merchantId && x.UserId == userId);
        if (merchant == null)
        {
            return FunctionResponse.Failure<MerchantDetailViewModel>(MessageCodes.MerchantNotFound);
        }

        ObjectId? categoryId = null;
        string categoryName = "Uncategorized";
        if (!string.IsNullOrWhiteSpace(request.CategoryId))
        {
            if (!ObjectId.TryParse(request.CategoryId, out var parsedId))
            {
                return FunctionResponse.Failure<MerchantDetailViewModel>(MessageCodes.InvalidCategoryId);
            }
            var category = await categoryRepository.GetAsync(x => x.Id == parsedId && (x.UserId == userId || x.IsSystem));
            if (category == null)
            {
                return FunctionResponse.Failure<MerchantDetailViewModel>(MessageCodes.CategoryNotFound);
            }
            categoryId = parsedId;
            categoryName = category.Name;
        }

        merchant.CategoryId = categoryId;
        merchant.UpdatedAt = DateTime.UtcNow;
        await merchantRepository.UpdateAsync(merchant);

        var response = new MerchantDetailViewModel
        {
            Id = merchant.Id.ToString(),
            Name = merchant.Name,
            CategoryId = categoryId?.ToString(),
            CategoryName = categoryName,
            TransactionCount = merchant.TransactionCount,
            TotalAmount = merchant.TransactionAmount
        };

        return FunctionResponse.Success(response);
    }

    private async Task<Dictionary<ObjectId, (int Count, decimal TotalAmount, DateTime? LastDate)>> GetTransactionStatsAsync(
        ObjectId userId,
        List<ObjectId> merchantIds,
        DateTime? startDate,
        DateTime? endDate)
    {
        if (merchantIds.Count == 0)
        {
            return new Dictionary<ObjectId, (int, decimal, DateTime?)>();
        }

        var transactions = await transactionRepository.ListAsync(x =>
            x.UserId == userId &&
            merchantIds.Contains(x.MerchantId) &&
            (!startDate.HasValue || x.Date >= startDate.Value) &&
            (!endDate.HasValue || x.Date <= endDate.Value));

        return transactions
            .GroupBy(x => x.MerchantId)
            .ToDictionary(
                g => g.Key,
                g => (g.Count(), g.Sum(x => x.Amount), g.Max(x => x.Date) as DateTime?));
    }

    private static List<MerchantListItemViewModel> ApplyMerchantSort(
        List<MerchantListItemViewModel> items,
        string? sortBy,
        string? sortDirection)
    {
        var sorted = items.AsEnumerable();
        if (sortBy == Constants.Finance.Sort.Amount)
        {
            sorted = sortDirection == Constants.Finance.Sort.Desc
                ? sorted.OrderByDescending(x => x.TotalAmount).ThenBy(x => x.Name)
                : sorted.OrderBy(x => x.TotalAmount).ThenBy(x => x.Name);
        }
        else if (sortBy == Constants.Finance.Sort.Transactions)
        {
            sorted = sortDirection == Constants.Finance.Sort.Desc
                ? sorted.OrderByDescending(x => x.TransactionCount).ThenBy(x => x.Name)
                : sorted.OrderBy(x => x.TransactionCount).ThenBy(x => x.Name);
        }
        else if (sortBy == Constants.Finance.Sort.Date)
        {
            sorted = sortDirection == Constants.Finance.Sort.Desc
                ? sorted.OrderByDescending(x => x.LastTransactionDate).ThenBy(x => x.Name)
                : sorted.OrderBy(x => x.LastTransactionDate).ThenBy(x => x.Name);
        }
        else
        {
            sorted = sortDirection == Constants.Finance.Sort.Desc
                ? sorted.OrderByDescending(x => x.Name)
                : sorted.OrderBy(x => x.Name);
        }

        return sorted.ToList();
    }
}

