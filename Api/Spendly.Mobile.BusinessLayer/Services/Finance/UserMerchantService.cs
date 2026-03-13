// CHANGED_BY_AI: 2026-03-02 - Add merchant nickname updates
// CHANGED_BY_AI: 2026-03-02 - Add merchant delete handler
// CHANGED_BY_AI: 2026-03-02 - Remove transaction aggregates from merchant responses
namespace Spendly.Mobile.BusinessLayer.Services.Finance;

using MongoDB.Bson;
using Shared.Core;
using Shared.Core.Extensions;
using Spendly.Mobile.BusinessLayer.Constants;
using Spendly.Mobile.ViewModels.Finance;
using Spendly.Shared.DataLayer;
using Spendly.Shared.Entities.TransactionManagement;
using Spendly.Shared.Entities.UserManagement;
using Spendly.Shared.Localization;
using Spendly.Shared.ViewModels;

public class UserMerchantService(
    IRepository<UserMerchant> userMerchantRepository,
    IRepository<Merchant> merchantRepository,
    IRepository<UserCategory> userCategoryRepository,
    RequestContextViewModel requestContextViewModel)
{
    //TODO review
    public async Task<FunctionResponse<List<MerchantListItemViewModel>>> ListAsync(MerchantListRequestViewModel request)
    {
        var userMerchants = (await userMerchantRepository.ListAsync(x => x.UserId == requestContextViewModel.UserId.ToObjectId())).ToList();

        if (userMerchants.Count == 0)
        {
            return FunctionResponse.Success(new List<MerchantListItemViewModel>());
        }

        var allUserCategories = await userCategoryRepository.ListDictionaryAsync(userMerchants.Where(p=>p.UserCategoryId.HasValue).Select(p => p.UserCategoryId!.Value));

        var allMerchants = await merchantRepository.ListDictionaryAsync(userMerchants.Select(p => p.MerchantId));

        var items = userMerchants.Select(x => new MerchantListItemViewModel
        {
            Id = x.Id.ToString(),
            Name = allMerchants[x.MerchantId].Name,
            Nickname = x.Nickname,
            CategoryId = x.UserCategoryId?.ToString(),
            CategoryName = x.UserCategoryId != null? allUserCategories[x.UserCategoryId.Value].Name : "",
            IsOther = x.IsOther
        })
        .OrderBy(p => p.IsOther)
            .ThenBy(p=>p.Name).ToList();

        return FunctionResponse.Success(items);
    }


    public async Task<FunctionResponse> DeleteAsync( string id)
    {
        if (!ObjectId.TryParse(id, out var merchantId))
        {
            return FunctionResponse.Failure(MessageCodes.MerchantNotFound);
        }

        var merchant = await userMerchantRepository.GetRequiredAsync(x => x.Id == merchantId && x.UserId == requestContextViewModel.UserId.ToObjectId());

        if (merchant.IsOther)
        {
            return FunctionResponse.Failure(MessageCodes.CannotModifyOtherMerchant);
        }

        await userMerchantRepository.DeleteAsync(merchant.Id);
        return FunctionResponse.Success();
    }


    public async Task<FunctionResponse<MerchantDetailViewModel>> UpdateAsync(string id, UserMerchantUpdateRequestViewModel request)
    {
        var userMerchant = await userMerchantRepository.GetRequiredAsync(x => x.Id == id.ToObjectId() && x.UserId == requestContextViewModel.UserId.ToObjectId());

        userMerchant.UserCategoryId = request.CategoryId?.ToObjectId();
        userMerchant.Nickname = request.Nickname;
        await userMerchantRepository.UpdateAsync(userMerchant);
        
        var merchant = await merchantRepository.GetRequiredAsync(x => x.Id == userMerchant.MerchantId);

        var response = new MerchantDetailViewModel
        {
            Id = userMerchant.Id.ToString(),
            Name = merchant.Name,
            Nickname = userMerchant.Nickname,
            CategoryId = userMerchant.UserCategoryId?.ToString(),
            IsOther = userMerchant.IsOther,
        };

        return FunctionResponse.Success(response);
    }
}
