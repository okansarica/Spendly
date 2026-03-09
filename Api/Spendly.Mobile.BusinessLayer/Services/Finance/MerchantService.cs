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
    IRepository<UserMerchant> userMerchantRepository,
    IRepository<Merchant> merchantRepository,
    IRepository<UserCategory> categoryRepository,
    RequestContextViewModel requestContextViewModel)
{
    //TODO review
    public async Task<FunctionResponse<List<MerchantListItemViewModel>>> ListAsync(MerchantListRequestViewModel request)
    {
        var userMerchants = (await userMerchantRepository.ListAsync(x => x.UserId == requestContextViewModel.UserId.ToObjectId())).ToList();

        if (request.IsUncategorized == true)
        {
            userMerchants = userMerchants.Where(x => x.UserCategoryId == null).ToList();
        }

        if (userMerchants.Count == 0)
        {
            return FunctionResponse.Success(new List<MerchantListItemViewModel>());
        }

        var categoryLookup = (await categoryRepository.ListAsync(x => x.UserId == requestContextViewModel.UserId.ToObjectId()))
            .ToDictionary(x => x.Id, x => x.Name);

        var allMerchants = await merchantRepository.ListDictionaryAsync(userMerchants.Select(p => p.MerchantId));

        var items = userMerchants.Select(x => new MerchantListItemViewModel
        {
            Id = x.Id.ToString(),
            Name = allMerchants[x.MerchantId].Name,
            Nickname = x.Nickname,
            CategoryId = x.UserCategoryId?.ToString(),
            CategoryName = x.UserCategoryId != null && categoryLookup.TryGetValue(x.UserCategoryId.Value, out var name) ? name : "Uncategorized",
        }).ToList();

        return FunctionResponse.Success(items);
    }


    public async Task<FunctionResponse> DeleteAsync( string id)
    {
        if (!ObjectId.TryParse(id, out var merchantId))
        {
            return FunctionResponse.Failure(MessageCodes.MerchantNotFound);
        }

        var merchant = await userMerchantRepository.GetRequiredAsync(x => x.Id == merchantId && x.UserId == requestContextViewModel.UserId.ToObjectId());

        await userMerchantRepository.DeleteAsync(merchant.Id);
        return FunctionResponse.Success();
    }


    public async Task<FunctionResponse<MerchantDetailViewModel>> UpdateAsync(string id, MerchantUpdateRequestViewModel request)
    {
        var merchantId = id.ToObjectIdOrNull();
        if (merchantId == null)
        {
            return FunctionResponse.Failure<MerchantDetailViewModel>(MessageCodes.MerchantNotFound);
        }

        var userMerchant = await userMerchantRepository.GetRequiredAsync(x => x.Id == merchantId && x.UserId == requestContextViewModel.UserId.ToObjectId());

        userMerchant.UserCategoryId = request.CategoryId?.ToObjectId();
        userMerchant.Nickname = request.Nickname;
        await userMerchantRepository.UpdateAsync(userMerchant);
        
        var merchant = await merchantRepository.GetRequiredAsync(x => x.Id == merchantId);

        var response = new MerchantDetailViewModel
        {
            Id = userMerchant.Id.ToString(),
            Name = merchant.Name,
            Nickname = userMerchant.Nickname,
            CategoryId = userMerchant.UserCategoryId?.ToString(),
        };

        return FunctionResponse.Success(response);
    }
}
