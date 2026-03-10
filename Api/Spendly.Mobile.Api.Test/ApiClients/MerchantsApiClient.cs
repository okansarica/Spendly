using System.Text;
using System.Text.Json;
using Spendly.Mobile.ViewModels.Finance;

namespace Spendly.Mobile.Api.Test.ApiClients;

public class MerchantsApiClient
{
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

    public MerchantsApiClient(HttpClient client)
    {
        _client = client;
    }

    private StringContent ToContent<T>(T obj)
    {
        var json = JsonSerializer.Serialize(obj);
        return new StringContent(json, Encoding.UTF8, "application/json");
    }

    public async Task<List<MerchantListItemViewModel>> ListAsync()
    {
        var resp = await _client.GetAsync("/api/v1/merchants");
        resp.EnsureSuccessStatusCode();
        var body = await resp.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<List<MerchantListItemViewModel>>(body, _jsonOptions)!;
    }

    public async Task<MerchantDetailViewModel> UpdateAsync(string id, UserMerchantUpdateRequestViewModel req)
    {
        var resp = await _client.PutAsync($"/api/v1/merchants/{id}", ToContent(req));
        resp.EnsureSuccessStatusCode();
        var body = await resp.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<MerchantDetailViewModel>(body, _jsonOptions)!;
    }

    public async Task<bool> DeleteAsync(string id)
    {
        var resp = await _client.DeleteAsync($"/api/v1/merchants/{id}");
        resp.EnsureSuccessStatusCode();
        return true;
    }
}
