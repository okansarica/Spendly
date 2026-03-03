// CHANGED_BY_AI: 2026-03-02 - Add merchant nickname updates
// CHANGED_BY_AI: 2026-03-02 - Add merchant delete handler
// CHANGED_BY_AI: 2026-03-02 - Remove transaction aggregates from merchant responses
namespace Spendly.Mobile.BusinessLayer.Services.Finance;

using MongoDB.Bson;
using Shared.Core;
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
    RequestContextViewModel requestContextViewModel)
{
    //TODO review
    public async Task<FunctionResponse<List<MerchantListItemViewModel>>> ListAsync(MerchantListRequestViewModel request)
    {
        var merchants = (await merchantRepository.ListAsync(x => x.UserId == requestContextViewModel.UserId.ToObjectId())).ToList();

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

        var categoryLookup = (await categoryRepository.ListAsync(x => x.UserId == requestContextViewModel.UserId.ToObjectId() || x.IsSystem))
            .ToDictionary(x => x.Id, x => x.Name);

        var items = merchants.Select(x => new MerchantListItemViewModel
        {
            Id = x.Id.ToString(),
            Name = x.Name,
            Nickname = x.Nickname,
            CategoryId = x.CategoryId?.ToString(),
            CategoryName = x.CategoryId != null && categoryLookup.TryGetValue(x.CategoryId.Value, out var name) ? name : "Uncategorized",
        }).ToList();

        var sorted = ApplyMerchantSort(items, request.SortBy, request.SortDirection);
        return FunctionResponse.Success(sorted);
    }


    public async Task<FunctionResponse> DeleteAsync( string id)
    {
        if (!ObjectId.TryParse(id, out var merchantId))
        {
            return FunctionResponse.Failure(MessageCodes.MerchantNotFound);
        }

        var merchant = await merchantRepository.GetRequiredAsync(x => x.Id == merchantId && x.UserId == requestContextViewModel.UserId.ToObjectId());

        await merchantRepository.DeleteAsync(merchant.Id);
        return FunctionResponse.Success();
    }


    public async Task<FunctionResponse<MerchantDetailViewModel>> UpdateAsync(string id, MerchantUpdateRequestViewModel request)
    {
        var merchantId = id.ToObjectIdOrNull();
        if (merchantId == null)
        {
            return FunctionResponse.Failure<MerchantDetailViewModel>(MessageCodes.MerchantNotFound);
        }

        var merchant = await merchantRepository.GetRequiredAsync(x => x.Id == merchantId && x.UserId == requestContextViewModel.UserId.ToObjectId());

        merchant.CategoryId = request.CategoryId?.ToObjectId();
        merchant.Nickname = request.Nickname;
        await merchantRepository.UpdateAsync(merchant);

        var response = new MerchantDetailViewModel
        {
            Id = merchant.Id.ToString(),
            Name = merchant.Name,
            Nickname = merchant.Nickname,
            CategoryId = merchant.CategoryId?.ToString(),
        };

        return FunctionResponse.Success(response);
    }

    private static List<MerchantListItemViewModel> ApplyMerchantSort(
        List<MerchantListItemViewModel> items,
        string? sortBy,
        string? sortDirection)
    {
        var sorted = items.AsEnumerable();
        if (sortBy == Constants.Finance.Sort.Name || string.IsNullOrWhiteSpace(sortBy))
        {
            sorted = sortDirection == Constants.Finance.Sort.Desc
                ? sorted.OrderByDescending(x => x.Name)
                : sorted.OrderBy(x => x.Name);
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
