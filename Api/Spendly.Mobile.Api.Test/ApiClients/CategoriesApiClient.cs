using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Spendly.Mobile.ViewModels.Finance;
using System.Collections.Generic;

namespace Spendly.Mobile.Api.Test.ApiClients;

public class CategoriesApiClient
{
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

    public CategoriesApiClient(HttpClient client)
    {
        _client = client;
    }

    private StringContent ToContent<T>(T obj)
    {
        var json = JsonSerializer.Serialize(obj);
        return new StringContent(json, Encoding.UTF8, "application/json");
    }

    public async Task<List<CategoryListItemViewModel>> ListAsync()
    {
        var resp = await _client.GetAsync("/api/v1/categories");
        resp.EnsureSuccessStatusCode();
        var body = await resp.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<List<CategoryListItemViewModel>>(body, _jsonOptions)!;
    }

    public async Task<CategoryResponseViewModel> CreateAsync(CategoryUpsertRequestViewModel req)
    {
        var resp = await _client.PostAsync("/api/v1/categories", ToContent(req));
        resp.EnsureSuccessStatusCode();
        var body = await resp.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<CategoryResponseViewModel>(body, _jsonOptions)!;
    }

    public async Task<CategoryResponseViewModel> UpdateAsync(string id, CategoryUpsertRequestViewModel req)
    {
        var resp = await _client.PutAsync($"/api/v1/categories/{id}", ToContent(req));
        resp.EnsureSuccessStatusCode();
        var body = await resp.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<CategoryResponseViewModel>(body, _jsonOptions)!;
    }

    public async Task<List<CategoryMerchantItemViewModel>> GetMerchantsAsync(string categoryId)
    {
        var resp = await _client.GetAsync($"/api/v1/categories/{categoryId}/merchants");
        resp.EnsureSuccessStatusCode();
        var body = await resp.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<List<CategoryMerchantItemViewModel>>(body, _jsonOptions)!;
    }

    public async Task<bool> DeleteAsync(string categoryId)
    {
        var resp = await _client.DeleteAsync($"/api/v1/categories/{categoryId}");
        resp.EnsureSuccessStatusCode();
        return true;
    }
}
