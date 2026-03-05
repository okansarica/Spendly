namespace Spendly.Mobile.Api.Test.ApiClients;

using Spendly.Mobile.ViewModels.Auth;
using System.Text;
using System.Text.Json;

public class AuthApiClient
{
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

    public AuthApiClient(HttpClient client)
    {
        _client = client;
    }

    private StringContent ToContent<T>(T obj)
    {
        var json = JsonSerializer.Serialize(obj);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        return content;
    }

    public async Task<AuthResponseViewModel> RegisterAsync(RegisterRequestViewModel req)
    {
        var resp = await _client.PostAsync("/api/v1/auth/register", ToContent(req));
        resp.EnsureSuccessStatusCode();
        var body = await resp.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<AuthResponseViewModel>(body, _jsonOptions)!;
    }

    public async Task<AuthResponseViewModel> VerifyEmailAsync(VerifyEmailRequestViewModel req)
    {
        var resp = await _client.PostAsync("/api/v1/auth/verify-email", ToContent(req));
        resp.EnsureSuccessStatusCode();
        var body = await resp.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<AuthResponseViewModel>(body, _jsonOptions)!;
    }

    public async Task<AuthResponseViewModel> LoginAsync(LoginRequestViewModel req)
    {
        var resp = await _client.PostAsync("/api/v1/auth/login", ToContent(req));
        if (!resp.IsSuccessStatusCode)
        {
            var err = await resp.Content.ReadAsStringAsync();
            throw new HttpRequestException($"Login failed {(int)resp.StatusCode}: {err}");
        }
        var body = await resp.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<AuthResponseViewModel>(body, _jsonOptions)!;
    }

    public async Task<bool> ForgotPasswordAsync(ForgotPasswordRequestViewModel req)
    {
        var resp = await _client.PostAsync("/api/v1/auth/forgot-password", ToContent(req));
        resp.EnsureSuccessStatusCode();
        return true;
    }

    public async Task<bool> ResendVerificationAsync(ResendCodeRequestViewModel req)
    {
        var resp = await _client.PostAsync("/api/v1/auth/resend-verification", ToContent(req));
        resp.EnsureSuccessStatusCode();
        var body = await resp.Content.ReadAsStringAsync();
        // returns { success = true }
        return body.Contains("true");
    }

    public async Task<AuthResponseViewModel> RefreshAccessTokenAsync(RefreshTokenRequestViewModel req)
    {
        var resp = await _client.PostAsync("/api/v1/auth/refresh-access-token", ToContent(req));
        if (!resp.IsSuccessStatusCode)
        {
            var err = await resp.Content.ReadAsStringAsync();
            throw new HttpRequestException($"Refresh failed {(int)resp.StatusCode}: {err}");
        }
        var body = await resp.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<AuthResponseViewModel>(body, _jsonOptions)!;
    }

    public async Task<bool> LogoutAsync()
    {
        var resp = await _client.PostAsync("/api/v1/auth/logout", new StringContent(string.Empty));
        resp.EnsureSuccessStatusCode();
        return true;
    }
}
