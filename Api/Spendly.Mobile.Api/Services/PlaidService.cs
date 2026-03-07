namespace Spendly.Mobile.Api.Services;

using BusinessLayer.Constants;
using Microsoft.AspNetCore.DataProtection;
using Shared.ViewModels;
using Spendly.Shared.ViewModels.Settings;
using Spendly.Shared.DataLayer;
using Spendly.Shared.Entities.Banking;
using System.Net.Http.Json;
using System.Text.Json;

public class PlaidService(
    HttpClient httpClient,
    IDataProtectionProvider dataProtectionProvider,
    IRepository<UserPlaidToken> userPlaidTokenRepository,
    IRepository<PlaidCommunicationLog> plaidCommunicationLogRepository,
    PlaidSettings plaidSettings,
    RequestContextViewModel requestContextViewModel,
    ILogger<PlaidService> logger)
{

    private readonly IDataProtector _protector =
        dataProtectionProvider.CreateProtector("UserPlaidTokenProtector");

    public async Task<string> CreateLinkTokenAsync()
    {
        var request = new
        {
            client_id = plaidSettings.ClientId,
            secret = plaidSettings.Secret,
            client_name = Constants.Application.ApplicationNAme,
            country_codes = new[] { "GB" },
            language = requestContextViewModel.Language,
            user = new { client_user_id = requestContextViewModel.UserId },
            products = new[] { "transactions" },
            redirect_uri = plaidSettings.RedirectUrl
        };

        var requestJson = JsonSerializer.Serialize(request);

        await plaidCommunicationLogRepository.InsertAsync(new PlaidCommunicationLog
        {
            Content = requestJson,
            Type = PlaidCommunicationLogType.CreateTLinkTokenRequest
        });

        var response = await httpClient.PostAsJsonAsync(
            plaidSettings.BaseUrl + "/link/token/create",
            request);

        var responseContent = await response.Content.ReadAsStringAsync();

        await plaidCommunicationLogRepository.InsertAsync(new PlaidCommunicationLog
        {
            Content = responseContent,
            Type = PlaidCommunicationLogType.CreateTLinkTokenResponse
        });

        if (!response.IsSuccessStatusCode)
        {
            logger.LogWarning(
                "Plaid link token create failed for user {UserId} with status {Status}. Response: {Response}",
                requestContextViewModel.UserId,
                response.StatusCode,
                responseContent);

            throw new Exception($"Plaid link token creation failed. Response: {responseContent}");
        }

        var body = JsonSerializer.Deserialize<JsonDocument>(responseContent);

        var linkToken = body!
            .RootElement
            .GetProperty("link_token")
            .GetString()!;

        return linkToken;
    }

    public async Task ExchangePublicTokenAsync(string publicToken)
    {
        var request = new
        {
            client_id = plaidSettings.ClientId,
            secret = plaidSettings.Secret,
            public_token = publicToken
        };

        var requestJson = JsonSerializer.Serialize(request);

        await plaidCommunicationLogRepository.InsertAsync(new PlaidCommunicationLog
        {
            Content = requestJson,
            Type = PlaidCommunicationLogType.ExchangePublicTokenRequest
        });

        var response = await httpClient.PostAsJsonAsync(
            plaidSettings.BaseUrl + "/item/public_token/exchange",
            request);

        var responseContent = await response.Content.ReadAsStringAsync();

        await plaidCommunicationLogRepository.InsertAsync(new PlaidCommunicationLog
        {
            Content = responseContent,
            Type = PlaidCommunicationLogType.ExchangePublicTokenResponse
        });

        if (!response.IsSuccessStatusCode)
        {
            logger.LogError(
                "Plaid public_token exchange failed for user {UserId} status {Status}. Response: {Response}",
                requestContextViewModel.UserId,
                response.StatusCode,
                responseContent);

            throw new Exception($"Plaid token exchange failed. Response: {responseContent}");
        }

        var body = JsonSerializer.Deserialize<JsonDocument>(responseContent);

        var accessToken = body!
            .RootElement
            .GetProperty("access_token")
            .GetString()!;

        var itemId = body
            .RootElement
            .GetProperty("item_id")
            .GetString()!;

        DateTime? expirationDateTime = null;

        if (body.RootElement.TryGetProperty("expiration", out var expProp))
        {
            expirationDateTime = expProp.GetDateTime();
        }

        var protectedToken = _protector.Protect(accessToken);

        var existingUserPlaidToken =
            await userPlaidTokenRepository.GetAsync(x => x.UserId == requestContextViewModel.UserId);

        if (existingUserPlaidToken != null)
        {
            existingUserPlaidToken.EncryptedAccessToken = protectedToken;
            existingUserPlaidToken.ItemId = itemId;
            existingUserPlaidToken.ExpirationDateTime = expirationDateTime;

            await userPlaidTokenRepository.UpdateAsync(existingUserPlaidToken);
        }
        else
        {
            var userPlaidToken = new UserPlaidToken
            {
                UserId = requestContextViewModel.UserId,
                EncryptedAccessToken = protectedToken,
                ItemId = itemId,
                ExpirationDateTime = expirationDateTime
            };

            await userPlaidTokenRepository.InsertAsync(userPlaidToken);
        }
    }
}
