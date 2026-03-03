// CHANGED_BY_AI: 2026-03-02 - Add category service
namespace Spendly.Mobile.BusinessLayer.Services.Finance;

using MongoDB.Bson;
using Spendly.Mobile.BusinessLayer.Constants;
using Spendly.Mobile.ViewModels.Finance;
using Spendly.Shared.Core;
using Spendly.Shared.DataLayer;
using Spendly.Shared.Entities.TransactionManagement;
using Spendly.Shared.Entities.UserManagement;
using Spendly.Shared.Localization;
using Spendly.Shared.ViewModels;

public class CategoryService(
    IRepository<Category> categoryRepository,
    IRepository<Merchant> merchantRepository,
    IRepository<NormalizedTransaction> transactionRepository,
    RequestContextViewModel requestContextViewModel)
{
    public async Task<FunctionResponse<List<CategoryListItemViewModel>>> GetListAsync(CategoryListRequestViewModel request)
    {
        var userId = requestContextViewModel.UserId.ToObjectId();
        var categories = (await categoryRepository.ListAsync(x => x.UserId == userId || x.IsSystem)).ToList();
        var search = request.Search?.Trim();
        if (!string.IsNullOrWhiteSpace(search))
        {
            categories = categories
                .Where(x => x.Name.Contains(search, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        var merchantCounts = (await merchantRepository.ListAsync(x => x.UserId == userId && x.CategoryId != null))
            .Where(x => x.CategoryId != null)
            .GroupBy(x => x.CategoryId!.Value)
            .ToDictionary(g => g.Key, g => g.Count());

        var sorted = ApplyCategorySort(categories, request.SortBy, request.SortDirection);

        var response = sorted.Select(x => new CategoryListItemViewModel
        {
            Id = x.Id.ToString(),
            Name = x.Name,
            ParentId = x.ParentId?.ToString(),
            Color = x.Color,
            Icon = x.Icon,
            MerchantCount = x.ParentId != null ? 0 : (merchantCounts.TryGetValue(x.Id, out var count) ? count : 0),
            IsSystem = x.IsSystem
        }).ToList();

        return FunctionResponse.Success(response);
    }

    public async Task<FunctionResponse<CategoryResponseViewModel>> CreateAsync(CategoryUpsertRequestViewModel request)
    {
        var userId = requestContextViewModel.UserId.ToObjectId();
        var name = request.Name?.Trim() ?? string.Empty;
        var existing = await categoryRepository.ListAsync(x => x.UserId == userId && x.Name.ToLower() == name.ToLower());
        if (existing.Any())
        {
            return FunctionResponse.Failure<CategoryResponseViewModel>(MessageCodes.DuplicateCategoryName);
        }

        ObjectId? parentId = null;
        if (!string.IsNullOrWhiteSpace(request.ParentId))
        {
            var parentObjectId = request.ParentId.ToObjectIdOrNull();
            if (parentObjectId == null)
            {
                return FunctionResponse.Failure<CategoryResponseViewModel>(MessageCodes.InvalidCategoryId);
            }
            var parent = await categoryRepository.GetAsync(x => x.Id == parentObjectId && x.UserId == userId);
            if (parent == null)
            {
                return FunctionResponse.Failure<CategoryResponseViewModel>(MessageCodes.CategoryNotFound);
            }
            parentId = parentObjectId;
        }

        var merchantIds = ParseMerchantIds(request.MerchantIds);
        if (merchantIds == null)
        {
            return FunctionResponse.Failure<CategoryResponseViewModel>(MessageCodes.MerchantNotFound);
        }

        if (merchantIds.Count > 0)
        {
            var merchants = (await merchantRepository.ListAsync(x => x.UserId == userId && merchantIds.Contains(x.Id))).ToList();
            if (merchants.Count != merchantIds.Count)
            {
                return FunctionResponse.Failure<CategoryResponseViewModel>(MessageCodes.MerchantNotFound);
            }
        }

        var category = new Category
        {
            UserId = userId,
            ParentId = parentId,
            Name = name,
            Color = request.Color,
            Icon = request.Icon,
            IsSystem = false
        };

        await categoryRepository.InsertAsync(category);

        if (merchantIds.Count > 0)
        {
            var merchants = await merchantRepository.ListAsync(x => x.UserId == userId && merchantIds.Contains(x.Id));
            foreach (var merchant in merchants)
            {
                merchant.CategoryId = category.Id;
                await merchantRepository.UpdateAsync(merchant);
            }
        }

        return FunctionResponse.Success(ToResponse(category, 0));
    }

    public async Task<FunctionResponse<CategoryResponseViewModel>> UpdateAsync(string id, CategoryUpsertRequestViewModel request)
    {
        var categoryId = id.ToObjectIdOrNull();
        if (categoryId == null)
        {
            return FunctionResponse.Failure<CategoryResponseViewModel>(MessageCodes.InvalidCategoryId);
        }

        var userId = requestContextViewModel.UserId.ToObjectId();
        var category = await categoryRepository.GetAsync(x => x.Id == categoryId && x.UserId == userId);
        if (category == null)
        {
            return FunctionResponse.Failure<CategoryResponseViewModel>(MessageCodes.CategoryNotFound);
        }

        var name = request.Name?.Trim() ?? string.Empty;
        var existing = await categoryRepository.ListAsync(x => x.UserId == userId && x.Id != category.Id && x.Name.ToLower() == name.ToLower());
        if (existing.Any())
        {
            return FunctionResponse.Failure<CategoryResponseViewModel>(MessageCodes.DuplicateCategoryName);
        }

        ObjectId? parentId = null;
        if (!string.IsNullOrWhiteSpace(request.ParentId))
        {
            var parentObjectId = request.ParentId.ToObjectIdOrNull();
            if (parentObjectId == null)
            {
                return FunctionResponse.Failure<CategoryResponseViewModel>(MessageCodes.InvalidCategoryId);
            }
            if (parentObjectId == category.Id)
            {
                return FunctionResponse.Failure<CategoryResponseViewModel>(MessageCodes.InvalidCategoryId);
            }
            var parent = await categoryRepository.GetAsync(x => x.Id == parentObjectId && x.UserId == userId);
            if (parent == null)
            {
                return FunctionResponse.Failure<CategoryResponseViewModel>(MessageCodes.CategoryNotFound);
            }
            parentId = parentObjectId;
        }

        category.Name = name;
        category.ParentId = parentId;
        category.Color = request.Color;
        category.Icon = request.Icon;
        category.UpdatedAt = DateTime.UtcNow;

        await categoryRepository.UpdateAsync(category);

        var merchantIds = ParseMerchantIds(request.MerchantIds);
        if (merchantIds == null)
        {
            return FunctionResponse.Failure<CategoryResponseViewModel>(MessageCodes.MerchantNotFound);
        }

        if (merchantIds.Count > 0)
        {
            var merchants = (await merchantRepository.ListAsync(x => x.UserId == userId && merchantIds.Contains(x.Id))).ToList();
            if (merchants.Count != merchantIds.Count)
            {
                return FunctionResponse.Failure<CategoryResponseViewModel>(MessageCodes.MerchantNotFound);
            }
            foreach (var merchant in merchants)
            {
                merchant.CategoryId = category.Id;
                await merchantRepository.UpdateAsync(merchant);
            }
        }

        var merchantCount = (await merchantRepository.ListAsync(x => x.UserId == userId && x.CategoryId == category.Id)).Count();
        return FunctionResponse.Success(ToResponse(category, merchantCount));
    }

    public async Task<FunctionResponse> DeleteAsync(string id)
    {
        var categoryId = id.ToObjectIdOrNull();
        if (categoryId == null)
        {
            return FunctionResponse.Failure(MessageCodes.InvalidCategoryId);
        }

        var userId = requestContextViewModel.UserId.ToObjectId();
        var category = await categoryRepository.GetAsync(x => x.Id == categoryId && x.UserId == userId);
        if (category == null)
        {
            return FunctionResponse.Failure(MessageCodes.CategoryNotFound);
        }

        var childCategories = await categoryRepository.ListAsync(x => x.UserId == userId && x.ParentId == categoryId);
        if (childCategories.Any())
        {
            return FunctionResponse.Failure(MessageCodes.CategoryHasChildren);
        }

        var merchants = await merchantRepository.ListAsync(x => x.UserId == userId && x.CategoryId == category.Id);
        foreach (var merchant in merchants)
        {
            merchant.CategoryId = null;
            await merchantRepository.UpdateAsync(merchant);
        }

        await categoryRepository.DeleteAsync(category.Id.ToString());
        return FunctionResponse.Success();
    }

    public async Task<FunctionResponse<List<CategoryMerchantItemViewModel>>> GetMerchantsAsync(string id)
    {
        var categoryId = id.ToObjectIdOrNull();
        if (categoryId == null)
        {
            return FunctionResponse.Failure<List<CategoryMerchantItemViewModel>>(MessageCodes.InvalidCategoryId);
        }

        var userId = requestContextViewModel.UserId.ToObjectId();
        var category = await categoryRepository.GetAsync(x => x.Id == categoryId && x.UserId == userId);
        if (category == null)
        {
            return FunctionResponse.Failure<List<CategoryMerchantItemViewModel>>(MessageCodes.CategoryNotFound);
        }

        var merchants = (await merchantRepository.ListAsync(x => x.UserId == userId && x.CategoryId == categoryId)).ToList();
        if (merchants.Count == 0)
        {
            return FunctionResponse.Success(new List<CategoryMerchantItemViewModel>());
        }

        var merchantIds = merchants.Select(x => x.Id).ToList();
        var transactions = await transactionRepository.ListAsync(x => x.UserId == userId && merchantIds.Contains(x.MerchantId));
        var transactionLookup = transactions
            .GroupBy(x => x.MerchantId)
            .ToDictionary(
                g => g.Key,
                g => new { Count = g.Count(), LastDate = g.Max(x => x.Date) });

        var response = merchants.Select(x => new CategoryMerchantItemViewModel
        {
            Id = x.Id.ToString(),
            Name = x.Name,
            TransactionCount = transactionLookup.TryGetValue(x.Id, out var stats) ? stats.Count : x.TransactionCount,
            LastTransactionDate = transactionLookup.TryGetValue(x.Id, out var stats2) ? stats2.LastDate : null
        }).ToList();

        return FunctionResponse.Success(response);
    }

    public async Task<FunctionResponse<CategoryResponseViewModel>> AddMerchantsAsync(string id, CategoryMerchantsRequestViewModel request)
    {
        var categoryId = id.ToObjectIdOrNull();
        if (categoryId == null)
        {
            return FunctionResponse.Failure<CategoryResponseViewModel>(MessageCodes.InvalidCategoryId);
        }

        var userId = requestContextViewModel.UserId.ToObjectId();
        var category = await categoryRepository.GetAsync(x => x.Id == categoryId && x.UserId == userId);
        if (category == null)
        {
            return FunctionResponse.Failure<CategoryResponseViewModel>(MessageCodes.CategoryNotFound);
        }

        var merchantIds = ParseMerchantIds(request.MerchantIds);
        if (merchantIds == null)
        {
            return FunctionResponse.Failure<CategoryResponseViewModel>(MessageCodes.MerchantNotFound);
        }

        if (merchantIds.Count > 0)
        {
            var merchants = (await merchantRepository.ListAsync(x => x.UserId == userId && merchantIds.Contains(x.Id))).ToList();
            if (merchants.Count != merchantIds.Count)
            {
                return FunctionResponse.Failure<CategoryResponseViewModel>(MessageCodes.MerchantNotFound);
            }

            foreach (var merchant in merchants)
            {
                merchant.CategoryId = category.Id;
                await merchantRepository.UpdateAsync(merchant);
            }
        }

        var merchantCount = (await merchantRepository.ListAsync(x => x.UserId == userId && x.CategoryId == category.Id)).Count();
        return FunctionResponse.Success(ToResponse(category, merchantCount));
    }

    public async Task<FunctionResponse> RemoveMerchantAsync(string id, string merchantId)
    {
        var categoryId = id.ToObjectIdOrNull();
        if (categoryId == null)
        {
            return FunctionResponse.Failure(MessageCodes.InvalidCategoryId);
        }

        var merchantObjectId = merchantId.ToObjectIdOrNull();
        if (merchantObjectId == null)
        {
            return FunctionResponse.Failure(MessageCodes.MerchantNotFound);
        }

        var userId = requestContextViewModel.UserId.ToObjectId();
        var category = await categoryRepository.GetAsync(x => x.Id == categoryId && x.UserId == userId);
        if (category == null)
        {
            return FunctionResponse.Failure(MessageCodes.CategoryNotFound);
        }

        var merchant = await merchantRepository.GetAsync(x => x.Id == merchantObjectId && x.UserId == userId);
        if (merchant == null)
        {
            return FunctionResponse.Failure(MessageCodes.MerchantNotFound);
        }

        if (merchant.CategoryId != categoryId)
        {
            return FunctionResponse.Failure(MessageCodes.CategoryMerchantLinkInvalid);
        }

        merchant.CategoryId = null;
        await merchantRepository.UpdateAsync(merchant);
        return FunctionResponse.Success();
    }

    private static List<Category> ApplyCategorySort(List<Category> categories, string? sortBy, string? sortDirection)
    {
        var sorted = categories.AsEnumerable();
        if (sortBy == Constants.Finance.Sort.Name || string.IsNullOrWhiteSpace(sortBy))
        {
            sorted = sortDirection == Constants.Finance.Sort.Desc
                ? sorted.OrderByDescending(x => x.Name)
                : sorted.OrderBy(x => x.Name);
        }
        return sorted.ToList();
    }

    private static List<ObjectId>? ParseMerchantIds(List<string>? merchantIds)
    {
        if (merchantIds == null)
        {
            return new List<ObjectId>();
        }

        var parsed = new List<ObjectId>();
        foreach (var id in merchantIds)
        {
            var objectId = id.ToObjectIdOrNull();
            if (objectId == null)
            {
                return null;
            }
            parsed.Add(objectId.Value);
        }
        return parsed;
    }

    private static CategoryResponseViewModel ToResponse(Category category, int merchantCount)
    {
        return new CategoryResponseViewModel
        {
            Id = category.Id.ToString(),
            Name = category.Name,
            ParentId = category.ParentId?.ToString(),
            Color = category.Color,
            Icon = category.Icon,
            MerchantCount = merchantCount,
            IsSystem = category.IsSystem
        };
    }
}

