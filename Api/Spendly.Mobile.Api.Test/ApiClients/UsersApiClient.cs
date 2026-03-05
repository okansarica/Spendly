namespace Spendly.Mobile.Api.Test.ApiClients;

using System.Text;
using System.Text.Json;
using Spendly.Mobile.ViewModels.User;

public class UsersApiClient
{
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

    public UsersApiClient(HttpClient client)
    {
        _client = client;
    }

    private StringContent ToContent<T>(T obj)
    {
        var json = JsonSerializer.Serialize(obj);
        return new StringContent(json, Encoding.UTF8, "application/json");
    }

    public async Task<UserProfileResponseViewModel> GetProfileAsync()
    {
        var resp = await _client.GetAsync("/api/v1/users/profile");
        resp.EnsureSuccessStatusCode();
        var body = await resp.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<UserProfileResponseViewModel>(body, _jsonOptions)!;
    }

    public async Task<UserProfileResponseViewModel> UpdateProfileAsync(UpdateUserProfileRequestViewModel req)
    {
        var resp = await _client.PutAsync("/api/v1/users/profile", ToContent(req));
        resp.EnsureSuccessStatusCode();
        var body = await resp.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<UserProfileResponseViewModel>(body, _jsonOptions)!;
    }

    public async Task<bool> ChangePasswordAsync(ChangePasswordRequestViewModel req)
    {
        var resp = await _client.PutAsync("/api/v1/users/change-password", ToContent(req));
        resp.EnsureSuccessStatusCode();
        return true;
    }

    public async Task<LanguagePreferenceResponseViewModel> SetLanguagePreferenceAsync(SetLanguagePreferenceRequestViewModel req)
    {
        var resp = await _client.PutAsync("/api/v1/users/language", ToContent(req));
        resp.EnsureSuccessStatusCode();
        var body = await resp.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<LanguagePreferenceResponseViewModel>(body, _jsonOptions)!;
    }

    public async Task<bool> DeleteAccountAsync()
    {
        var resp = await _client.DeleteAsync("/api/v1/users/account");
        resp.EnsureSuccessStatusCode();
        return true;
    }

    public async Task<DateTime?> GetSubscriptionEndDateAsync()
    {
        var resp = await _client.GetAsync("/api/v1/users/subscription-end");
        resp.EnsureSuccessStatusCode();
        var body = await resp.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(body);
        if (doc.RootElement.TryGetProperty("subscriptionEndDateTime", out var p))
            return p.GetDateTime();
        return null;
    }

    public async Task<List<SubscriptionPlanResponseViewModel>> GetSubscriptionPlansAsync()
    {
        var resp = await _client.GetAsync("/api/v1/users/subscription-plans");
        resp.EnsureSuccessStatusCode();
        var body = await resp.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<List<SubscriptionPlanResponseViewModel>>(body, _jsonOptions)!;
    }

    public async Task<CreatePaymentUrlResponseViewModel> CreatePaymentUrlAsync(CreatePaymentUrlRequestViewModel req)
    {
        var resp = await _client.PostAsync("/api/v1/users/create-payment-url", ToContent(req));
        resp.EnsureSuccessStatusCode();
        var body = await resp.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<CreatePaymentUrlResponseViewModel>(body, _jsonOptions)!;
    }

    public async Task<bool> SaveFirebaseTokenAsync(SaveFirebaseTokenRequest req)
    {
        var resp = await _client.PostAsync("/api/v1/users/firebase-token", ToContent(req));
        resp.EnsureSuccessStatusCode();
        return true;
    }
}

