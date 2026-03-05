using Bogus;
using MongoDB.Driver;
using Spendly.Mobile.ViewModels.Auth;
using System.Net.Http.Headers;
using Microsoft.Extensions.DependencyInjection;
using Spendly.Shared.Entities.Auth;
using Spendly.Shared.DataLayer;

namespace Spendly.Mobile.Api.Test;

using ApiClients;
using TestHelpers;

public class TestFixture : IAsyncDisposable
{
    public WebApplicationFactory Factory { get; }
    public HttpClient Client { get; }
    public IMongoClient MongoClient { get; }
    public string? GlobalAccessToken { get; private set; }
    public AuthApiClient AuthClient { get; }
    public CategoriesApiClient CategoriesClient => new CategoriesApiClient(CreateAuthenticatedClient(GlobalAccessToken ?? string.Empty));
    public MerchantsApiClient MerchantsClient => new MerchantsApiClient(CreateAuthenticatedClient(GlobalAccessToken ?? string.Empty));
    public UsersApiClient UsersClient => new UsersApiClient(CreateAuthenticatedClient(GlobalAccessToken ?? string.Empty));

    public TestFixture()
    {
        Factory = new WebApplicationFactory();
        Client = Factory.CreateClient();
        AuthClient = new AuthApiClient(Client);

        var port = int.Parse(Environment.GetEnvironmentVariable("TEST_MONGO_PORT")!);
        var host = Environment.GetEnvironmentVariable("TEST_MONGO_HOST") ?? "localhost";
        MongoClient = new MongoClient($"mongodb://{host}:{port}");
    }

    public HttpClient CreateAuthenticatedClient(string token)
    {
        var c = Factory.CreateClient();
        c.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return c;
    }

    public async Task<string> RegisterAndLoginAsync(RegisterRequestViewModel register)
    {
        var regResponse = await AuthClient.RegisterAsync(register);

        // Mark user email as verified in test Mongo so login returns tokens
        var dbName = Environment.GetEnvironmentVariable("DbSettings__DatabaseName") ?? "SpendlyTestDb";
        var db = MongoClient.GetDatabase(dbName);
        var users = db.GetCollection<MongoDB.Bson.BsonDocument>("User");
        var filter = Builders<MongoDB.Bson.BsonDocument>.Filter.Eq("Email", register.Email);
        var update = Builders<MongoDB.Bson.BsonDocument>.Update.Set("EmailVerification.IsVerified", true);
        await users.UpdateOneAsync(filter, update);

        // After register, perform login
        var loginRequest = new LoginRequestViewModel { Email = register.Email, Password = register.Password, FirebaseToken = register.FirebaseToken ?? "test-firebase-token" };
        var loginResponse = await AuthClient.LoginAsync(loginRequest);
        GlobalAccessToken = loginResponse.AccessToken;
        return loginResponse.AccessToken!;
    }

    public async Task<AuthResponseViewModel> RegisterAndVerifyAsync(RegisterRequestViewModel register)
    {
        var registerResponse = await AuthClient.RegisterAsync(register);

        var userRepository = Factory.Services.GetRequiredService<IRepository<User>>();
        var user = await userRepository.GetAsync(u => u.Email == register.Email);
        if (user == null)
            throw new Exception($"User with email {register.Email} not found in test DB after register");

        var code = user.EmailVerification.VerificationCode;
        if (string.IsNullOrEmpty(code))
            throw new Exception("Verification code not set for registered user");

        var verifyRequest = new VerifyEmailRequestViewModel { UserId = user.Id.ToString(), Code = code };
        var verifyResp = await AuthClient.VerifyEmailAsync(verifyRequest);
        GlobalAccessToken = verifyResp?.AccessToken;

        // Adjust user's trial subscription to be expired so Login endpoint (which currently checks subscription) will succeed in tests
        var userSubscriptionRepository = Factory.Services.GetRequiredService<IRepository<Spendly.Shared.Entities.Subscription.UserSubscription>>();
        var subscriptions = await userSubscriptionRepository.ListAsync(p => p.UserId == user.Id);
        if (subscriptions != null && subscriptions.Count > 0)
        {
            var firstSubscription = subscriptions[0];
            firstSubscription.ExpectedEndDateTime = DateTime.UtcNow.AddDays(-1);
            await userSubscriptionRepository.UpdateAsync(firstSubscription);
        }

        return verifyResp!;
    }

    public async Task<FixtureVerifiedUser> PrepareVerifiedUserAsync(RegisterRequestViewModel? register = null)
    {
        if (register == null)
            register = GenerateRegisterRequest();

        var verified = await RegisterAndVerifyAsync(register);

        // fetch user id from repo
        var userRepository = Factory.Services.GetRequiredService<IRepository<User>>();
        var user = await userRepository.GetAsync(u => u.Email == register.Email);
        if (user == null)
            throw new Exception("User not found after registration");

        return new FixtureVerifiedUser
        {
            Register = register,
            UserId = user.Id.ToString(),
            AccessToken = verified.AccessToken ?? string.Empty,
            RefreshToken = verified.RefreshToken ?? string.Empty
        };
    }

    public async Task<(RegisterRequestViewModel registerRequest, AuthResponseViewModel verified)> WithVerifiedUser()
    {
        var registerRequest = GenerateRegisterRequest();
        var verified = await RegisterAndVerifyAsync(registerRequest);
        return (registerRequest, verified);
    }

    public RegisterRequestViewModel GenerateRegisterRequest()
    {
        var faker = new Faker();
        return new RegisterRequestViewModel
        {
            Name = faker.Person.FirstName,
            Surname = faker.Person.LastName,
            Email = faker.Internet.Email().ToLowerInvariant(),
            Password = "P@ssw0rd1234",
            FirebaseToken = Guid.NewGuid().ToString(),
        };
    }

    public async ValueTask DisposeAsync()
    {
        Factory.Dispose();
        Client.Dispose();
        await Task.CompletedTask;
    }
}
