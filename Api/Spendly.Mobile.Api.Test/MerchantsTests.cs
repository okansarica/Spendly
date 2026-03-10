using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Spendly.Shared.DataLayer;
using Spendly.Shared.Entities.TransactionManagement;
using Spendly.Mobile.ViewModels.Finance;
using MongoDB.Bson;

namespace Spendly.Mobile.Api.Test;

using ApiClients;

public class MerchantsTests(TestFixture fixture) : IClassFixture<TestFixture>
{
    [Fact]
    public async Task MerchantLifecycleIncludesListUpdateAndDelete()
    {
        var verifiedUser = await fixture.PrepareVerifiedUserAsync();
        verifiedUser.Should().NotBeNull();

        var merchantsClient = fixture.MerchantsClient;

        var merchantRepo = fixture.Factory.Services.GetRequiredService<IRepository<Merchant>>();
        var userMerchantRepo = fixture.Factory.Services.GetRequiredService<IRepository<UserMerchant>>();

        var predefined = new Merchant { Name = "Predef Merchant", IsOther = false };
        await merchantRepo.InsertAsync(predefined);

        var userMerchant = new UserMerchant { UserId = ObjectId.Parse(verifiedUser.UserId), MerchantId = predefined.Id, Nickname = null, IsOther = false };
        await userMerchantRepo.InsertAsync(userMerchant);

        var list = await merchantsClient.ListAsync();
        list.Should().ContainSingle(x => x.Id == userMerchant.Id.ToString());

        var updateRequest = new UserMerchantUpdateRequestViewModel { Nickname = "Nick", CategoryId = null };
        var updated = await merchantsClient.UpdateAsync(userMerchant.Id.ToString(), updateRequest);
        updated.Should().NotBeNull();
        updated.Nickname.Should().Be("Nick");

        // Ensure underlying predefined merchant name did not change
        var reloadedPredefined = await merchantRepo.GetAsync(predefined.Id.ToString());
        reloadedPredefined.Name.Should().Be("Predef Merchant");

        // Cannot update when IsOther == true
        var otherPredefined = new Merchant { Name = "Other", IsOther = true };
        await merchantRepo.InsertAsync(otherPredefined);
        var otherUserMerchant = new UserMerchant { UserId = ObjectId.Parse(verifiedUser.UserId), MerchantId = otherPredefined.Id, IsOther = true };
        await userMerchantRepo.InsertAsync(otherUserMerchant);

        var failUpdate = async () => await merchantsClient.UpdateAsync(otherUserMerchant.Id.ToString(), updateRequest);
        await failUpdate.Should().ThrowAsync<Exception>();

        var deleted = await merchantsClient.DeleteAsync(userMerchant.Id.ToString());
        deleted.Should().BeTrue();

        var listAfterDelete = await merchantsClient.ListAsync();
        listAfterDelete.Should().NotContain(x => x.Id == userMerchant.Id.ToString());
    }
}
