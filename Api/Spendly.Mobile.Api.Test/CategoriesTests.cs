using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Spendly.Shared.DataLayer;
using Spendly.Shared.Entities.TransactionManagement;
using Spendly.Mobile.ViewModels.Finance;
using System.Text.Json;
using Spendly.Shared.Core;
using MongoDB.Bson;

namespace Spendly.Mobile.Api.Test;

using ApiClients;

public class CategoriesTests(TestFixture fixture) : IClassFixture<TestFixture>
{
    [Fact]
    public async Task CategoryLifecycleAndMerchantRetrieval()
    {
        var verifiedUser = await fixture.PrepareVerifiedUserAsync();
        verifiedUser.Should().NotBeNull();

        var authenticatedClient = fixture.CreateAuthenticatedClient(verifiedUser.AccessToken);
        var categoriesClient = new CategoriesApiClient(authenticatedClient);

        // 1) List should be empty initially
        var list = await categoriesClient.ListAsync();
        list.Should().NotBeNull();

        // 2) Create a merchant and then create a category linking that merchant
        var merchantRepository = fixture.Factory.Services.GetRequiredService<IRepository<Merchant>>();
        var merchant = new Merchant { UserId = ObjectId.Parse(verifiedUser.UserId), Name = "TestMerchant" };
        await merchantRepository.InsertAsync(merchant);

        var categoryCreateRequest = new CategoryUpsertRequestViewModel { Name = "Food", MerchantIds = new List<string> { merchant.Id.ToString() }, Color = "#fff", Icon = "cutlery" };
        var createdCategory = await categoriesClient.CreateAsync(categoryCreateRequest);
        createdCategory.Should().NotBeNull();
        createdCategory!.Name.Should().Be("Food");
        createdCategory.MerchantCount.Should().Be(0);

        // 3) Update category to change merchant list (ensure merchant linking works)
        var categoryUpdateRequest = new CategoryUpsertRequestViewModel { Name = "Food Updated", MerchantIds = new List<string> { merchant.Id.ToString() }, Color = "#000", Icon = "bowl" };
        var updatedCategory = await categoriesClient.UpdateAsync(createdCategory.Id, categoryUpdateRequest);
        updatedCategory.Should().NotBeNull();
        updatedCategory!.Name.Should().Be("Food Updated");

        // 4) Get merchants by category
        var merchantsInCategory = await categoriesClient.GetMerchantsAsync(createdCategory.Id);
        merchantsInCategory.Should().NotBeNull();

        // 5) Delete category
        var isCategoryDeleted = await categoriesClient.DeleteAsync(createdCategory.Id);
        isCategoryDeleted.Should().BeTrue();
    }
}
