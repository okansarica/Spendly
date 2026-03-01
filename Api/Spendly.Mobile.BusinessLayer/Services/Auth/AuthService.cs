namespace Spendly.Mobile.BusinessLayer.Services.Auth;

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.IdentityModel.Tokens;
using Spendly.Mobile.ViewModels.Auth;
using Spendly.Shared.Core;
using Spendly.Shared.DataLayer;
using Spendly.Shared.Entities.Auth;
using Spendly.Shared.Enums;
using Spendly.Shared.Localization;
using Spendly.Shared.ViewModels;
using Spendly.Shared.ViewModels.Settings;

public class AuthService(
    IRepository<User> userRepository,
    IRepository<UserRefreshToken> refreshTokenRepository,
    JwtSettings jwtSettings,
    IHttpClientFactory httpClientFactory)
{
    private const int MaxVerificationAttempts = 5;
    private const int VerificationCodeExpiryHours = 24;

    public async Task<FunctionResponse<AuthResponseViewModel>> LoginAsync(LoginRequestViewModel request)
    {
        var email = request.Email.ToLowerInvariant();
        var user = await userRepository.GetAsync(u => u.Email == email);

        if (user == null || !user.LoginProviders.Any(p => p.Provider == LoginProviderType.Local))
            return FunctionResponse.Failure<AuthResponseViewModel>(MessageCodes.InvalidCredentials);

        if (!user.IsActive)
            return FunctionResponse.Failure<AuthResponseViewModel>(MessageCodes.InvalidCredentials);

        if (!PasswordHelper.VerifyPassword(request.Password, user.PasswordHash))
            return FunctionResponse.Failure<AuthResponseViewModel>(MessageCodes.InvalidCredentials);

        if (!user.EmailVerification.IsVerified)
        {
            if (user.EmailVerification.IsVerificationLocked)
                return FunctionResponse.Failure<AuthResponseViewModel>(MessageCodes.AccountLocked);

            var codeExpired = user.EmailVerification.LastCodeIssuedAt == null ||
                              (DateTime.UtcNow - user.EmailVerification.LastCodeIssuedAt.Value).TotalHours > VerificationCodeExpiryHours;

            if (codeExpired)
            {
                user.EmailVerification.VerificationCode = GenerateVerificationCode();
                user.EmailVerification.LastCodeIssuedAt = DateTime.UtcNow;
                user.UpdatedAt = DateTime.UtcNow;
                await userRepository.UpdateAsync(user);
            }

            return FunctionResponse.Success(new AuthResponseViewModel
            {
                Id = user.Id.ToString(),
                Email = user.Email,
                EmailVerificationRequired = true
            });
        }

        var (accessToken, accessTokenExpiry, refreshToken, refreshTokenExpiry) = GenerateTokens(user);
        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiry = refreshTokenExpiry;
        user.UpdatedAt = DateTime.UtcNow;
        await userRepository.UpdateAsync(user);
        await SaveRefreshTokenAsync(user.Id, refreshToken, refreshTokenExpiry);

        return FunctionResponse.Success(new AuthResponseViewModel
        {
            Id = user.Id.ToString(),
            Email = user.Email,
            AccessToken = accessToken,
            AccessTokenExpire = accessTokenExpiry,
            RefreshToken = refreshToken,
            RefreshTokenExpire = refreshTokenExpiry,
            EmailVerificationRequired = false
        });
    }

    public async Task<FunctionResponse<AuthResponseViewModel>> SocialLoginAsync(SocialLoginRequestViewModel request)
    {
        var providerType = request.Provider.ToLowerInvariant() switch
        {
            "google" => (LoginProviderType?)LoginProviderType.Google,
            "facebook" => LoginProviderType.Facebook,
            _ => null
        };

        if (providerType == null)
            return FunctionResponse.Failure<AuthResponseViewModel>(MessageCodes.InvalidToken);

        SocialUserInfo? socialUser = providerType == LoginProviderType.Google
            ? await ValidateGoogleTokenAsync(request.Token)
            : await ValidateFacebookTokenAsync(request.Token);

        if (socialUser == null)
            return FunctionResponse.Failure<AuthResponseViewModel>(MessageCodes.InvalidToken);

        var email = socialUser.Email.ToLowerInvariant();
        var user = await userRepository.GetAsync(u => u.Email == email);

        if (user == null)
        {
            user = new User
            {
                Email = email,
                EmailVerification = new EmailVerification { IsVerified = true },
                LoginProviders = new List<UserLoginProvider>
                {
                    new() { Provider = providerType.Value, ProviderUserId = socialUser.ProviderId }
                },
                IsActive = true
            };
            await userRepository.InsertAsync(user);
        }
        else
        {
            if (!user.LoginProviders.Any(p => p.Provider == providerType.Value))
            {
                user.LoginProviders.Add(new UserLoginProvider { Provider = providerType.Value, ProviderUserId = socialUser.ProviderId });
                user.UpdatedAt = DateTime.UtcNow;
                await userRepository.UpdateAsync(user);
            }
        }

        var (accessToken, accessTokenExpiry, refreshToken, refreshTokenExpiry) = GenerateTokens(user);
        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiry = refreshTokenExpiry;
        user.UpdatedAt = DateTime.UtcNow;
        await userRepository.UpdateAsync(user);
        await SaveRefreshTokenAsync(user.Id, refreshToken, refreshTokenExpiry);

        return FunctionResponse.Success(new AuthResponseViewModel
        {
            Id = user.Id.ToString(),
            Email = user.Email,
            AccessToken = accessToken,
            AccessTokenExpire = accessTokenExpiry,
            RefreshToken = refreshToken,
            RefreshTokenExpire = refreshTokenExpiry,
            EmailVerificationRequired = false
        });
    }

    public async Task<FunctionResponse> ForgotPasswordAsync(ForgotPasswordRequestViewModel request)
    {
        var email = request.Email.ToLowerInvariant();
        var user = await userRepository.GetAsync(u => u.Email == email);

        if (user != null && user.LoginProviders.Any(p => p.Provider == LoginProviderType.Local))
        {
            user.EmailVerification.VerificationCode = GenerateVerificationCode();
            user.EmailVerification.LastCodeIssuedAt = DateTime.UtcNow;
            user.UpdatedAt = DateTime.UtcNow;
            await userRepository.UpdateAsync(user);
            // TODO: Trigger email delivery with reset link
        }

        return FunctionResponse.Success();
    }

    private (string accessToken, DateTime accessTokenExpiry, string refreshToken, DateTime refreshTokenExpiry) GenerateTokens(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var accessTokenExpiry = DateTime.UtcNow.AddHours(1);
        var refreshTokenExpiry = DateTime.UtcNow.AddDays(7);

        var claims = new[]
        {
            new Claim(ClaimTypes.Name, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: jwtSettings.Issuer,
            audience: jwtSettings.Audience,
            claims: claims,
            expires: accessTokenExpiry,
            signingCredentials: creds);

        var accessToken = new JwtSecurityTokenHandler().WriteToken(token);
        var refreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

        return (accessToken, accessTokenExpiry, refreshToken, refreshTokenExpiry);
    }

    private async Task SaveRefreshTokenAsync(MongoDB.Bson.ObjectId userId, string refreshToken, DateTime expiry)
    {
        var tokenEntity = new UserRefreshToken
        {
            UserId = userId,
            Token = refreshToken,
            ExpireDateTime = expiry
        };
        await refreshTokenRepository.InsertAsync(tokenEntity);
    }

    private static string GenerateVerificationCode() =>
        Random.Shared.Next(100000, 999999).ToString();

    private async Task<SocialUserInfo?> ValidateGoogleTokenAsync(string idToken)
    {
        var client = httpClientFactory.CreateClient();
        var response = await client.GetAsync($"https://oauth2.googleapis.com/tokeninfo?id_token={idToken}");
        if (!response.IsSuccessStatusCode) return null;

        var json = await response.Content.ReadAsStringAsync();
        var data = JsonSerializer.Deserialize<JsonElement>(json);

        if (!data.TryGetProperty("email", out var emailProp) || !data.TryGetProperty("sub", out var subProp))
            return null;

        return new SocialUserInfo { Email = emailProp.GetString()!, ProviderId = subProp.GetString()! };
    }

    private async Task<SocialUserInfo?> ValidateFacebookTokenAsync(string accessToken)
    {
        var client = httpClientFactory.CreateClient();
        var response = await client.GetAsync($"https://graph.facebook.com/me?access_token={accessToken}&fields=id,email");
        if (!response.IsSuccessStatusCode) return null;

        var json = await response.Content.ReadAsStringAsync();
        var data = JsonSerializer.Deserialize<JsonElement>(json);

        if (!data.TryGetProperty("email", out var emailProp) || !data.TryGetProperty("id", out var idProp))
            return null;

        return new SocialUserInfo { Email = emailProp.GetString()!, ProviderId = idProp.GetString()! };
    }

    private sealed class SocialUserInfo
    {
        public string Email { get; set; } = null!;
        public string ProviderId { get; set; } = null!;
    }
}

