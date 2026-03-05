using FluentAssertions;
using System.Net.Http;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Spendly.Shared.Entities.Auth;
using Spendly.Shared.DataLayer;
using Spendly.Mobile.ViewModels.Auth;

namespace Spendly.Mobile.Api.Test;

using ApiClients;

public class AuthTests(TestFixture fixture) : IClassFixture<TestFixture>
{

    [Fact]
    public async Task RegisterThenVerifyReturnsToken()
    {
        var authClient = new AuthApiClient(fixture.Client);
        var registerRequest = fixture.GenerateRegisterRequest();

        var registerResponse = await authClient.RegisterAsync(registerRequest);
        registerResponse.Should().NotBeNull();
        registerResponse.EmailVerificationRequired.Should().BeTrue();

        var userRepository = fixture.Factory.Services.GetRequiredService<IRepository<User>>();
        var user = await userRepository.GetAsync(u => u.Email == registerRequest.Email);
        user.Should().NotBeNull();

        var code = user!.EmailVerification.VerificationCode;
        code.Should().NotBeNullOrEmpty();
        var userId = user.Id.ToString();

        var verifyResponse = await authClient.VerifyEmailAsync(new VerifyEmailRequestViewModel { UserId = userId, Code = code! });
        verifyResponse.Should().NotBeNull();
        verifyResponse.AccessToken.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task RegisterAndVerifyThenLoginReturnsTokens()
    {
        var registerRequest = fixture.GenerateRegisterRequest();

        var verifiedResponse = await fixture.RegisterAndVerifyAsync(registerRequest);
        verifiedResponse.Should().NotBeNull();
        verifiedResponse.AccessToken.Should().NotBeNullOrEmpty();

        var authClient = new AuthApiClient(fixture.Client);
        var loginResponse = await authClient.LoginAsync(new LoginRequestViewModel { Email = registerRequest.Email, Password = registerRequest.Password, FirebaseToken = registerRequest.FirebaseToken });
        loginResponse.Should().NotBeNull();
        loginResponse.AccessToken.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task ForgotPasswordAllowsRequestAndResendGeneratesCode()
    {
        var authClient = new AuthApiClient(fixture.Client);
        var registerRequest = fixture.GenerateRegisterRequest();

        var verifiedResponse = await fixture.RegisterAndVerifyAsync(registerRequest);
        verifiedResponse.Should().NotBeNull();

        var forgotResult = await authClient.ForgotPasswordAsync(new ForgotPasswordRequestViewModel { Email = registerRequest.Email });
        forgotResult.Should().BeTrue();

        var userRepository = fixture.Factory.Services.GetRequiredService<IRepository<User>>();
        var user = await userRepository.GetAsync(u => u.Email == registerRequest.Email);
        user.Should().NotBeNull();
        user!.EmailVerification.VerificationCode.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task ResendVerificationReturnsSuccess()
    {
        var authClient = new AuthApiClient(fixture.Client);
        var registerRequest = fixture.GenerateRegisterRequest();

        var registerResponse = await authClient.RegisterAsync(registerRequest);
        registerResponse.EmailVerificationRequired.Should().BeTrue();

        var userRepository = fixture.Factory.Services.GetRequiredService<IRepository<User>>();
        var user = await userRepository.GetAsync(u => u.Email == registerRequest.Email);
        user.Should().NotBeNull();

        var resendResult = await authClient.ResendVerificationAsync(new ResendCodeRequestViewModel { UserId = user!.Id.ToString() });
        resendResult.Should().BeTrue();
    }

    [Fact]
    public async Task RefreshAccessTokenReturnsNewTokens()
    {
        var authApiClient = new AuthApiClient(fixture.Client);
        var verifiedUser = await fixture.PrepareVerifiedUserAsync();
        verifiedUser.Should().NotBeNull();

        var refreshToken = verifiedUser.RefreshToken;
        refreshToken.Should().NotBeNullOrEmpty();

        var refreshedTokens = await authApiClient.RefreshAccessTokenAsync(new RefreshTokenRequestViewModel { RefreshToken = refreshToken! });
        refreshedTokens.Should().NotBeNull();
        refreshedTokens.AccessToken.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task LogoutDeletesRefreshTokens()
    {
        var verifiedUser = await fixture.PrepareVerifiedUserAsync();
        verifiedUser.Should().NotBeNull();

        var accessToken = verifiedUser.AccessToken;
        accessToken.Should().NotBeNullOrEmpty();

        var authenticatedClient = fixture.CreateAuthenticatedClient(accessToken!);
        var authApiClient = new AuthApiClient(authenticatedClient);
        var logoutResult = await authApiClient.LogoutAsync();
        logoutResult.Should().BeTrue();

        //todo refresh tokenin db den silindiginden emin ol, respotiory ile kaydi incele
    }
    
    //TODO olumsuz senaryo da test istiyorum db de refresh token yoksa yenileneyemsin
}
