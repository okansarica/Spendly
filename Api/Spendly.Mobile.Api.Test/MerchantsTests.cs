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

        var merchantRepository = fixture.Factory.Services.GetRequiredService<IRepository<UserMerchant>>();

        var merchant = new UserMerchant { UserId = ObjectId.Parse(verifiedUser.UserId)}; //TODO fix
        await merchantRepository.InsertAsync(merchant);

        var list = await merchantsClient.ListAsync();
        list.Should().ContainSingle(x => x.Id == merchant.Id.ToString());

        var updateRequest = new MerchantUpdateRequestViewModel { Nickname = "Nick", CategoryId = null };
        var updated = await merchantsClient.UpdateAsync(merchant.Id.ToString(), updateRequest);
        updated.Should().NotBeNull();
        updated.Nickname.Should().Be("Nick");

        var deleted = await merchantsClient.DeleteAsync(merchant.Id.ToString());
        deleted.Should().BeTrue();

        var listAfterDelete = await merchantsClient.ListAsync();
        listAfterDelete.Should().NotContain(x => x.Id == merchant.Id.ToString());
    }
}
