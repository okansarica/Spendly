// CHANGED_BY_AI: 2026-03-03 - Add logout integration and register defaults
namespace Spendly.Mobile.BusinessLayer.Services.Auth;

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.IdentityModel.Tokens;
using Shared.Entities.Subscription;
using Shared.Entities.UserManagement;
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
	IRepository<UserSubscription> userSubscriptionRepository,
	IRepository<UserRefreshToken> refreshTokenRepository,
	IRepository<FirebaseToken> firebaseTokenRepository,
	JwtSettings jwtSettings,
	IHttpClientFactory httpClientFactory,
	RequestContextViewModel requestContextViewModel)
{
	private const int MaxVerificationAttempts = 5;
	private const int VerificationCodeExpiryHours = 24;

	public async Task<FunctionResponse<AuthResponseViewModel>> LoginAsync(LoginRequestViewModel request)
	{
		var email = request.Email.ToLowerInvariant();
		var user = await userRepository.GetAsync(u => u.Email == email);

		if (user == null ||
		    !user.LoginProviders.Any(p => p.Provider == LoginProviderType.Local))
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
				EmailVerificationRequired = true,
				LanguageCode = user.LanguageCode
			});
		}

		var (accessToken, accessTokenExpiry, refreshToken, refreshTokenExpiry) = GenerateTokens(user);
		await SaveRefreshTokenAsync(user.Id, refreshToken, refreshTokenExpiry);

		var userSubscriptions = await userSubscriptionRepository.ListAsync(p => p.UserId == user.Id).ConfigureAwait(false);

		var activeSubscription =
			userSubscriptions.SingleOrDefault(p =>
				p.SubscriptionType == SubscriptionType.Paid&&
				p.StartDateTime.HasValue &&
				p.StartDateTime.Value >= DateTime.UtcNow &&
				((!p.EndDateTime.HasValue && p.ExpectedEndDateTime > DateTime.UtcNow) || (p.EndDateTime.HasValue && p.ExpectedEndDateTime > DateTime.UtcNow)));

		DateTime? subscriptionEndDate = null;
		if (activeSubscription == null)
		{
			var trialSubscriptions = userSubscriptions.Single(p => p.SubscriptionType == SubscriptionType.Trial);
			if ((trialSubscriptions.EndDateTime.HasValue && trialSubscriptions.EndDateTime.Value <= DateTime.UtcNow) ||
			    trialSubscriptions.ExpectedEndDateTime <= DateTime.UtcNow)
			{
				return FunctionResponse.Failure<AuthResponseViewModel>(MessageCodes.NoActiveSubscription);
			}
			
			subscriptionEndDate = trialSubscriptions.EndDateTime??trialSubscriptions.ExpectedEndDateTime;
		}

		if (!string.IsNullOrEmpty(request.FirebaseToken))
		{
			var firebaseToken = await firebaseTokenRepository.GetAsync(p => p.Token == request.FirebaseToken);
			if (firebaseToken == null)
			{
				firebaseToken = new FirebaseToken
				{
					Token = request.FirebaseToken,
					UserId = user.Id
				};
				await firebaseTokenRepository.InsertAsync(firebaseToken).ConfigureAwait(false);
			}
			else if (!firebaseToken.UserId.HasValue || firebaseToken.UserId.Value != user.Id)
			{
				firebaseToken.UserId = user.Id;
				await firebaseTokenRepository.UpdateAsync(firebaseToken).ConfigureAwait(false);
			}
		}

		return FunctionResponse.Success(new AuthResponseViewModel
		{
			Id = user.Id.ToString(),
			Email = user.Email,
			AccessToken = accessToken,
			AccessTokenExpire = accessTokenExpiry,
			RefreshToken = refreshToken,
			RefreshTokenExpire = refreshTokenExpiry,
			EmailVerificationRequired = false,
			LanguageCode = user.LanguageCode,
			SubscriptionEndDateTime = subscriptionEndDate,
		});
	}

	public async Task<FunctionResponse<AuthResponseViewModel>> SocialLoginAsync(SocialLoginRequestViewModel request)
	{
		var providerType = request.Provider;

		SocialUserInfo? socialUser = providerType switch
		{
			LoginProviderType.Google => await ValidateGoogleTokenAsync(request.Token),
			LoginProviderType.Facebook => await ValidateFacebookTokenAsync(request.Token),
			_ => null
		};

		if (socialUser == null)
			return FunctionResponse.Failure<AuthResponseViewModel>(MessageCodes.InvalidToken);

		var email = socialUser.Email.ToLowerInvariant();
		var user = await userRepository.GetAsync(u => u.Email == email);

		if (user == null)
		{
			user = new User
			{
				Email = email,
				EmailVerification = new EmailVerification {IsVerified = true},
				LoginProviders = new List<UserLoginProvider>
				{
					new() {Provider = providerType, ProviderUserId = socialUser.ProviderId}
				},
				IsActive = true
			};
			await userRepository.InsertAsync(user);
		}
		else
		{
			if (!user.LoginProviders.Any(p => p.Provider == providerType))
			{
				user.LoginProviders.Add(new UserLoginProvider {Provider = providerType, ProviderUserId = socialUser.ProviderId});
				user.UpdatedAt = DateTime.UtcNow;
				await userRepository.UpdateAsync(user);
			}
		}

		var (accessToken, accessTokenExpiry, refreshToken, refreshTokenExpiry) = GenerateTokens(user);
		await SaveRefreshTokenAsync(user.Id, refreshToken, refreshTokenExpiry);
		
		if (!string.IsNullOrEmpty(request.FirebaseToken))
		{
			var firebaseToken = await firebaseTokenRepository.GetAsync(p => p.Token == request.FirebaseToken);
			if (firebaseToken == null)
			{
				firebaseToken = new FirebaseToken
				{
					Token = request.FirebaseToken,
					UserId = user.Id
				};
				await firebaseTokenRepository.InsertAsync(firebaseToken).ConfigureAwait(false);
			}
			else if (!firebaseToken.UserId.HasValue || firebaseToken.UserId.Value != user.Id)
			{
				firebaseToken.UserId = user.Id;
				await firebaseTokenRepository.UpdateAsync(firebaseToken).ConfigureAwait(false);
			}
		}

		return FunctionResponse.Success(new AuthResponseViewModel
		{
			Id = user.Id.ToString(),
			Email = user.Email,
			AccessToken = accessToken,
			AccessTokenExpire = accessTokenExpiry,
			RefreshToken = refreshToken,
			RefreshTokenExpire = refreshTokenExpiry,
			EmailVerificationRequired = false,
			LanguageCode = user.LanguageCode
		});
	}

	public async Task<FunctionResponse> ForgotPasswordAsync(ForgotPasswordRequestViewModel request)
	{
		var email = request.Email.ToLowerInvariant();
		var user = await userRepository.GetAsync(u => u.Email == email);

		if (user != null &&
		    user.LoginProviders.Any(p => p.Provider == LoginProviderType.Local))
		{
			user.EmailVerification.VerificationCode = GenerateVerificationCode();
			user.EmailVerification.LastCodeIssuedAt = DateTime.UtcNow;
			user.UpdatedAt = DateTime.UtcNow;
			await userRepository.UpdateAsync(user);

		}

		return FunctionResponse.Success();
	}

	public async Task<FunctionResponse<AuthResponseViewModel>> RegisterAsync(RegisterRequestViewModel request)
	{
		var email = request.Email.ToLowerInvariant();
		var existingUser = await userRepository.GetAsync(u => u.Email == email);

		if (existingUser != null)
		{
			return FunctionResponse.Failure<AuthResponseViewModel>(MessageCodes.EmailAlreadyExists);
		}

		var verificationCode = GenerateVerificationCode();
		var user = new User
		{
			Name = request.Name,
			Surname = request.Surname,
			Email = email,
			PasswordHash = PasswordHelper.HashPassword(request.Password),
			EmailVerification = new EmailVerification
			{
				IsVerified = false,
				VerificationCode = verificationCode,
				LastCodeIssuedAt = DateTime.UtcNow,
				VerificationAttemptCount = 0,
				IsVerificationLocked = false
			},
			LoginProviders = new List<UserLoginProvider>
			{
				new() {Provider = LoginProviderType.Local}
			},
			IsActive = true,
			IsNewsletterSubscribed = false,
			LanguageCode = "en"
		};
		await userRepository.InsertAsync(user).ConfigureAwait(false);

		var userSubscription = new UserSubscription
		{
			ExpectedEndDateTime = DateTime.UtcNow.AddDays(Constants.Constants.User.TrialDurationInDays),
			StartDateTime = DateTime.UtcNow,
			SubscriptionType = SubscriptionType.Trial,
			UserId = user.Id,
		};
		await userSubscriptionRepository.InsertAsync(userSubscription).ConfigureAwait(false);

		DateTime? subscriptionEndDate = userSubscription.EndDateTime ?? userSubscription.ExpectedEndDateTime;
		
		if (!string.IsNullOrEmpty(request.FirebaseToken))
		{
			var firebaseToken = await firebaseTokenRepository.GetAsync(p => p.Token == request.FirebaseToken);
			if (firebaseToken == null)
			{
				firebaseToken = new FirebaseToken
				{
					Token = request.FirebaseToken,
					UserId = user.Id
				};
				await firebaseTokenRepository.InsertAsync(firebaseToken).ConfigureAwait(false);
			}
			else if (!firebaseToken.UserId.HasValue || firebaseToken.UserId.Value != user.Id)
			{
				firebaseToken.UserId = user.Id;
				await firebaseTokenRepository.UpdateAsync(firebaseToken).ConfigureAwait(false);
			}
		}
		
		return FunctionResponse.Success(new AuthResponseViewModel
		{
			Id = user.Id.ToString(),
			Email = user.Email,
			EmailVerificationRequired = true,
			LanguageCode = user.LanguageCode,
			SubscriptionEndDateTime = subscriptionEndDate
		});
		
		//TODO cateegory, merchant kaydi eklenecek, isOther
	}

	public async Task<FunctionResponse<AuthResponseViewModel>> RefreshAccessTokenAsync(RefreshTokenRequestViewModel request)
	{
		var refreshToken = request.RefreshToken;

		var tokenEntity = await refreshTokenRepository.GetAsync(t => t.Token == refreshToken);

		if (tokenEntity == null ||
		    tokenEntity.ExpireDateTime < DateTime.UtcNow)
			return FunctionResponse.Failure<AuthResponseViewModel>(MessageCodes.InvalidToken);

		var user = await userRepository.GetAsync(u => u.Id == tokenEntity.UserId);

		if (user == null ||
		    !user.IsActive)
			return FunctionResponse.Failure<AuthResponseViewModel>(MessageCodes.InvalidToken);

		var (accessToken, accessTokenExpiry, newRefreshToken, newRefreshTokenExpiry) = GenerateTokens(user);

		var shouldRenewRefreshToken = (DateTime.UtcNow - tokenEntity.CreatedAt).TotalDays >= jwtSettings.RefreshRenewDays;

		if (shouldRenewRefreshToken)
		{
			await refreshTokenRepository.DeleteAsync(tokenEntity.Id);
			await SaveRefreshTokenAsync(user.Id, newRefreshToken, newRefreshTokenExpiry);
		}
		else
		{
			newRefreshToken = refreshToken;
			newRefreshTokenExpiry = tokenEntity.ExpireDateTime;
		}

		user.UpdatedAt = DateTime.UtcNow;
		await userRepository.UpdateAsync(user);

		return FunctionResponse.Success(new AuthResponseViewModel
		{
			Id = user.Id.ToString(),
			Email = user.Email,
			AccessToken = accessToken,
			AccessTokenExpire = accessTokenExpiry,
			RefreshToken = newRefreshToken,
			RefreshTokenExpire = newRefreshTokenExpiry,
			EmailVerificationRequired = false,
			LanguageCode = user.LanguageCode
		});
	}

	public async Task<FunctionResponse<AuthResponseViewModel>> VerifyEmailAsync(VerifyEmailRequestViewModel request)
	{
		if (!MongoDB.Bson.ObjectId.TryParse(request.UserId, out var userId))
		{
			return FunctionResponse.Failure<AuthResponseViewModel>(MessageCodes.UserNotFound);
		}

		var user = await userRepository.GetAsync(u => u.Id == userId);

		if (user == null)
		{
			return FunctionResponse.Failure<AuthResponseViewModel>(MessageCodes.UserNotFound);
		}

		if (user.EmailVerification.IsVerificationLocked)
		{
			return FunctionResponse.Failure<AuthResponseViewModel>(MessageCodes.AccountLocked);
		}

		if (user.EmailVerification.IsVerified)
		{
			return FunctionResponse.Failure<AuthResponseViewModel>(MessageCodes.InvalidVerificationCode);
		}

		var codeExpired = user.EmailVerification.LastCodeIssuedAt == null ||
		                  (DateTime.UtcNow - user.EmailVerification.LastCodeIssuedAt.Value).TotalHours > VerificationCodeExpiryHours;

		if (codeExpired)
		{
			return FunctionResponse.Failure<AuthResponseViewModel>(MessageCodes.VerificationCodeExpired);
		}

		if (user.EmailVerification.VerificationCode != request.Code)
		{
			user.EmailVerification.VerificationAttemptCount++;

			if (user.EmailVerification.VerificationAttemptCount >= MaxVerificationAttempts)
			{
				user.EmailVerification.IsVerificationLocked = true;
			}

			user.UpdatedAt = DateTime.UtcNow;
			await userRepository.UpdateAsync(user);

			if (user.EmailVerification.IsVerificationLocked)
			{
				return FunctionResponse.Failure<AuthResponseViewModel>(MessageCodes.TooManyVerificationAttempts);
			}

			return FunctionResponse.Failure<AuthResponseViewModel>(MessageCodes.InvalidVerificationCode);
		}

		user.EmailVerification.IsVerified = true;
		user.EmailVerification.VerificationCode = null;
		user.EmailVerification.VerificationAttemptCount = 0;
		user.UpdatedAt = DateTime.UtcNow;
		await userRepository.UpdateAsync(user);

		var (accessToken, accessTokenExpiry, refreshToken, refreshTokenExpiry) = GenerateTokens(user);

		await SaveRefreshTokenAsync(user.Id, refreshToken, refreshTokenExpiry);

		return FunctionResponse.Success(new AuthResponseViewModel
		{
			Id = user.Id.ToString(),
			Email = user.Email,
			AccessToken = accessToken,
			AccessTokenExpire = accessTokenExpiry,
			RefreshToken = refreshToken,
			RefreshTokenExpire = refreshTokenExpiry,
			EmailVerificationRequired = false,
			LanguageCode = user.LanguageCode
		});
	}

	public async Task<FunctionResponse> ResendCodeAsync(ResendCodeRequestViewModel request)
	{
		if (!MongoDB.Bson.ObjectId.TryParse(request.UserId, out var userId))
		{
			return FunctionResponse.Failure(MessageCodes.UserNotFound);
		}

		var user = await userRepository.GetAsync(u => u.Id == userId);

		if (user == null)
		{
			return FunctionResponse.Failure(MessageCodes.UserNotFound);
		}

		if (user.EmailVerification.IsVerificationLocked)
		{
			return FunctionResponse.Failure(MessageCodes.AccountLocked);
		}

		if (user.EmailVerification.IsVerified)
		{
			return FunctionResponse.Success();
		}

		user.EmailVerification.VerificationCode = GenerateVerificationCode();
		user.EmailVerification.LastCodeIssuedAt = DateTime.UtcNow;
		user.EmailVerification.VerificationAttemptCount = 0;
		user.UpdatedAt = DateTime.UtcNow;
		await userRepository.UpdateAsync(user);
		
		//TODO send e mail

		return FunctionResponse.Success();
	}

	public async Task<FunctionResponse> LogoutAsync()
	{
		var user = await userRepository.GetRequiredAsync(x => x.Id == requestContextViewModel.UserId.ToObjectId());

		var refreshTokens = await refreshTokenRepository.ListAsync(x => x.UserId == requestContextViewModel.UserId.ToObjectId());
		foreach (var token in refreshTokens)
		{
			await refreshTokenRepository.DeleteAsync(token.Id);
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

	private static string GenerateVerificationCode() => Random.Shared.Next(1000, 9999).ToString();

	private async Task<SocialUserInfo?> ValidateGoogleTokenAsync(string idToken)
	{
		var client = httpClientFactory.CreateClient();
		var response = await client.GetAsync($"https://oauth2.googleapis.com/tokeninfo?id_token={idToken}");
		if (!response.IsSuccessStatusCode)
			return null;

		var json = await response.Content.ReadAsStringAsync();
		var data = JsonSerializer.Deserialize<JsonElement>(json);

		if (!data.TryGetProperty("email", out var emailProp) ||
		    !data.TryGetProperty("sub", out var subProp))
			return null;

		return new SocialUserInfo {Email = emailProp.GetString()!, ProviderId = subProp.GetString()!};
	}

	private async Task<SocialUserInfo?> ValidateFacebookTokenAsync(string accessToken)
	{
		var client = httpClientFactory.CreateClient();
		var response = await client.GetAsync($"https://graph.facebook.com/me?access_token={accessToken}&fields=id,email");
		if (!response.IsSuccessStatusCode)
			return null;

		var json = await response.Content.ReadAsStringAsync();
		var data = JsonSerializer.Deserialize<JsonElement>(json);

		if (!data.TryGetProperty("email", out var emailProp) ||
		    !data.TryGetProperty("id", out var idProp))
			return null;

		return new SocialUserInfo {Email = emailProp.GetString()!, ProviderId = idProp.GetString()!};
	}

	private sealed class SocialUserInfo
	{
		public string Email { get; set; } = null!;
		public string ProviderId { get; set; } = null!;
	}
}
