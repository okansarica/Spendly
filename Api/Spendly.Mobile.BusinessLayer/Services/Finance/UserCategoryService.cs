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

public class UserCategoryService(
	IRepository<UserCategory> categoryRepository,
	IRepository<UserMerchant> userMerchantRepository,
	IRepository<Merchant> merchantRepository,
	RequestContextViewModel requestContextViewModel)
{
	public async Task<FunctionResponse<List<CategoryListItemViewModel>>> GetListAsync(CategoryListRequestViewModel request)
	{
		var userId = requestContextViewModel.UserId.ToObjectId();
		var categories = (await categoryRepository.ListAsync(x => x.UserId == userId)).ToList();
		var sorted =  categories.OrderBy(x => x.IsOther).ThenBy(p=>p.Name).ToList();
		var response = sorted.Select(x => new CategoryListItemViewModel
			{
				Id = x.Id.ToString(),
				Name = x.Name,
				ParentId = x.ParentId?.ToString(),
				Color = x.Color,
				Icon = x.Icon,
				MerchantCount = x.MerchantCount,
				IsOther = x.IsOther,
			})
			.ToList();

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

		var merchantIds = request.MerchantIds.Select(p => p.ToObjectId()).ToList();

		if (merchantIds.Count > 0)
		{
			var merchants = (await userMerchantRepository.ListAsync(x => x.UserId == userId && merchantIds.Contains(x.Id))).ToList();
			if (merchants.Count != merchantIds.Count)
			{
				return FunctionResponse.Failure<CategoryResponseViewModel>(MessageCodes.MerchantNotFound);
			}
		}

		var category = new UserCategory
		{
			UserId = userId,
			ParentId = parentId,
			Name = name,
			Color = request.Color,
			Icon = request.Icon,
		};

		await categoryRepository.InsertAsync(category);

		if (merchantIds.Count > 0)
		{
			var merchants = await userMerchantRepository.ListAsync(x => x.UserId == userId && merchantIds.Contains(x.Id));
			foreach (var merchant in merchants)
			{
				merchant.UserCategoryId = category.Id;
				await userMerchantRepository.UpdateAsync(merchant);
			}
		}

		return FunctionResponse.Success(ToResponse(category, 0));
	}

	public async Task<FunctionResponse<CategoryResponseViewModel>> UpdateAsync(string id, CategoryUpsertRequestViewModel request)
	{
		var categoryId = id.ToObjectId();
		var userId = requestContextViewModel.UserId.ToObjectId();

		var category = await categoryRepository.GetAsync(x => x.Id == categoryId && x.UserId == userId);
		if (category == null)
		{
			return FunctionResponse.Failure<CategoryResponseViewModel>(MessageCodes.CategoryNotFound);
		}

		if (category.IsOther)
		{
			return FunctionResponse.Failure<CategoryResponseViewModel>(MessageCodes.CannotModifyOtherCategory);
		}

		var name = request.Name?.Trim() ?? string.Empty;

		var existing = await categoryRepository.ListAsync(x => x.UserId == userId &&
		                                                       x.Id != category.Id &&
		                                                       x.Name.ToLower() == name.ToLower());

		if (existing.Any())
		{
			return FunctionResponse.Failure<CategoryResponseViewModel>(MessageCodes.DuplicateCategoryName);
		}

		ObjectId? parentId = null;
		if (!string.IsNullOrWhiteSpace(request.ParentId))
		{
			var parentObjectId = request.ParentId.ToObjectId();

			if (parentObjectId == category.Id)
			{
				return FunctionResponse.Failure<CategoryResponseViewModel>(MessageCodes.InvalidCategoryId);
			}

			_ = await categoryRepository.GetRequiredAsync(x => x.Id == parentObjectId && x.UserId == userId);

			parentId = parentObjectId;
		}

		category.Name = name;
		category.ParentId = parentId;
		category.Color = request.Color;
		category.Icon = request.Icon;
		category.UpdatedAt = DateTime.UtcNow;

		await categoryRepository.UpdateAsync(category);

		// ---------------------------
		// Merchant işlemleri
		// ---------------------------

		var merchants = (await userMerchantRepository.ListAsync(x => x.UserId == userId &&
		                                                         request.MerchantIds.Contains(x.Id.ToString())))
			.ToList();

		if (merchants.Count != request.MerchantIds.Count)
		{
			return FunctionResponse.Failure<CategoryResponseViewModel>(MessageCodes.MerchantNotFound);
		}

		var selectedMerchantIds = request.MerchantIds.ToHashSet();

		var existingLinkedMerchants = await userMerchantRepository.ListAsync(x => x.UserId == userId &&
		                                                                      x.UserCategoryId == category.Id);

		// Unlink edilenler
		foreach (var linkedMerchant in existingLinkedMerchants)
		{
			if (!selectedMerchantIds.Contains(linkedMerchant.Id.ToString()))
			{
				linkedMerchant.UserCategoryId = null;
				await userMerchantRepository.UpdateAsync(linkedMerchant);
			}
		}

		// Yeni linklenenler
		foreach (var merchant in merchants)
		{
			if (merchant.UserCategoryId != category.Id)
			{
				merchant.UserCategoryId = category.Id;
				await userMerchantRepository.UpdateAsync(merchant);
			}
		}

		// ---------------------------
		// ✅ MerchantCount güncelle
		// ---------------------------

		var finalMerchantCount = (await userMerchantRepository.ListAsync(x => x.UserId == userId &&
		                                                                  x.UserCategoryId == category.Id)).Count();

		category.MerchantCount = finalMerchantCount;
		category.UpdatedAt = DateTime.UtcNow;

		await categoryRepository.UpdateAsync(category);

		return FunctionResponse.Success(
			ToResponse(category, finalMerchantCount));
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

		if (category.IsOther)
		{
			return FunctionResponse.Failure(MessageCodes.CannotModifyOtherCategory);
		}

		var childCategories = await categoryRepository.ListAsync(x => x.UserId == userId && x.ParentId == categoryId);
		if (childCategories.Any())
		{
			return FunctionResponse.Failure(MessageCodes.CategoryHasChildren);
		}

		var merchants = await userMerchantRepository.ListAsync(x => x.UserId == userId && x.UserCategoryId == category.Id);
		foreach (var merchant in merchants)
		{
			merchant.UserCategoryId = null;
			await userMerchantRepository.UpdateAsync(merchant);
		}

		await categoryRepository.DeleteAsync(category.Id.ToString());
		return FunctionResponse.Success();
	}

	public async Task<FunctionResponse<List<CategoryMerchantItemViewModel>>> GetMerchantsAsync(string id)
	{
		var categoryId = id.ToObjectId();

		var userId = requestContextViewModel.UserId.ToObjectId();
		var category = await categoryRepository.GetAsync(x => x.Id == categoryId && x.UserId == userId);
		if (category == null)
		{
			return FunctionResponse.Failure<List<CategoryMerchantItemViewModel>>(MessageCodes.CategoryNotFound);
		}

		var userMerchants = (await userMerchantRepository.ListAsync(x => x.UserId == userId && x.UserCategoryId == categoryId)).ToList();
		if (userMerchants.Count == 0)
		{
			return FunctionResponse.Success(new List<CategoryMerchantItemViewModel>());
		}

		var allMerchants = await merchantRepository.ListDictionaryAsync(userMerchants.Select(p => p.MerchantId));
		var response = userMerchants.Select(x => new CategoryMerchantItemViewModel
			{
				Id = x.Id.ToString(),
				Name = allMerchants[x.MerchantId].Name,
			})
			.ToList();

		return FunctionResponse.Success(response);
	}

	private static List<UserCategory> ApplyCategorySort(List<UserCategory> categories, string? sortBy, string? sortDirection)
	{
		var sorted = categories.AsEnumerable();
		if (sortBy == Constants.Finance.Sort.Name ||
		    string.IsNullOrWhiteSpace(sortBy))
		{
			sorted = sortDirection == Constants.Finance.Sort.Desc ? sorted.OrderByDescending(x => x.Name) : sorted.OrderBy(x => x.Name);
		}
		return sorted.ToList();
	}


	private static CategoryResponseViewModel ToResponse(UserCategory userCategory, int merchantCount)
	{
		return new CategoryResponseViewModel
		{
			Id = userCategory.Id.ToString(),
			Name = userCategory.Name,
			ParentId = userCategory.ParentId?.ToString(),
			Color = userCategory.Color,
			Icon = userCategory.Icon,
			MerchantCount = merchantCount,
			IsOther = userCategory.IsOther,
		};
	}
}
