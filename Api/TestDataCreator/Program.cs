using MongoDB.Bson;
using MongoDB.Driver;
using Spendly.Shared.Core;
using Spendly.Shared.Entities.Auth;
using Spendly.Shared.Entities.TransactionManagement;
using Spendly.Shared.Entities.UserManagement;
using Spendly.Shared.Entities.Reporting;
using Spendly.Shared.Entities.Subscription;
using Spendly.Shared.Enums;

namespace TestDataCreator;

class Program
{
	private static MongoClient _client = null!;
	private static IMongoDatabase _database = null!;

	static async Task Main(string[] args)
	{
		try
		{
			InitializeDatabase();
			await GenerateTestData();
			Console.WriteLine("\n✓ Test data generation completed successfully!");
		}
		catch (Exception ex)
		{
			Console.WriteLine($"✗ Error: {ex.Message}");
		}
	}

	static void InitializeDatabase()
	{
		var settings = new MongoClientSettings
		{
			Server = new MongoServerAddress("localhost", 27017),
			Credential = MongoCredential.CreateCredential("Spendly", "SpendlyUser", "1234")
		};

		_client = new MongoClient(settings);
		_database = _client.GetDatabase("Spendly");
		Console.WriteLine("✓ Database connection established");
	}

	static async Task GenerateTestData()
	{
		
		
		var users = await CreateUsers();
		await CreateUserSubscriptions(users);
		await CreateCategories(users);
	}
	

	static async Task<List<User>> CreateUsers()
	{
		var collection = _database.GetCollection<User>("User");
		await collection.DeleteManyAsync(FilterDefinition<User>.Empty);

		var passwordHash = HashPassword("123");

		var users = new List<User>
		{
			new User
			{
				Id = ObjectId.GenerateNewId(),
				Name = "Ahmet",
				Surname = "Yilmaz",
				Email = "ahmet@example.com",
				PasswordHash = passwordHash,
				IsActive = true,
				EmailVerification = new EmailVerification { IsVerified = true },
				LoginProviders = [new UserLoginProvider
				{
					Provider = LoginProviderType.Local,
				}]
			},
			new User
			{
				Id = ObjectId.GenerateNewId(),
				Name = "Fatih",
				Surname = "Kaya",
				Email = "fatih@example.com",
				PasswordHash = passwordHash,
				IsActive = true,
				EmailVerification = new EmailVerification { IsVerified = true }
			},
			new User
			{
				Id = ObjectId.GenerateNewId(),
				Name = "Zeynep",
				Surname = "Demir",
				Email = "zeynep@example.com",
				PasswordHash = passwordHash,
				IsActive = true,
				EmailVerification = new EmailVerification { IsVerified = true },
				LoginProviders = [new UserLoginProvider
				{
					Provider = LoginProviderType.Local,
				}]
			},
			new User
			{
				Id = ObjectId.GenerateNewId(),
				Name = "Okan",
				Surname = "Sarica",
				Email = "okansarica@gmail.com",
				PasswordHash = passwordHash,
				IsActive = true,
				EmailVerification = new EmailVerification { IsVerified = true },
				LoginProviders = [new UserLoginProvider
				{
					Provider = LoginProviderType.Local,
				}]
			}
		};

		await collection.InsertManyAsync(users);
		Console.WriteLine($"✓ Created {users.Count} users");
		return users;
	}

	static string HashPassword(string password)
	{
		return PasswordHelper.HashPassword(password);
	}


	static async Task CreateUserSubscriptions(List<User> users)
	{
		var collection = _database.GetCollection<UserSubscription>("UserSubscription");
		await collection.DeleteManyAsync(FilterDefinition<UserSubscription>.Empty);

		var subscriptions = new List<UserSubscription>();
		var now = DateTime.UtcNow;
		foreach (var user in users)
		{
			var start = now;
			var expectedEnd = start.AddDays(5);
			subscriptions.Add(new UserSubscription
			{
				Id = ObjectId.GenerateNewId(),
				UserId = user.Id,
				StartDateTime = start,
				ExpectedEndDateTime = expectedEnd,
				EndDateTime = null,
				Payment = new UserSubscriptionPayment
				{
					Duration = UserSubscriptionDurationType.Monthly,
					Amount = 0m,
					PaymentStatus = UserSubscriptionPaymentStatusType.Paid
				},
				SubscriptionType = SubscriptionType.Trial,
				CreatedAt = DateTime.UtcNow
			});
		}

		if (subscriptions.Any())
		{
			await collection.InsertManyAsync(subscriptions);
		}

		Console.WriteLine($"✓ Created {subscriptions.Count} trial subscriptions");
	}

	static async Task CreateCategories(List<User> users)
	{
		var collection = _database.GetCollection<Category>("Category");
		await collection.DeleteManyAsync(FilterDefinition<Category>.Empty);

		var categories = new List<Category>();
		var categoryData = new[]
		{
			new { Name = "Food & Dining", Color = "#FF6B6B", Icon = "🍔" },
			new { Name = "Transportation", Color = "#4ECDC4", Icon = "🚗" },
			new { Name = "Shopping", Color = "#45B7D1", Icon = "🛍️" },
			new { Name = "Entertainment", Color = "#FFA07A", Icon = "🎬" },
			new { Name = "Bills & Utilities", Color = "#98D8C8", Icon = "💡" }
		};

		foreach (var user in users)
		{
			foreach (var data in categoryData)
			{
				categories.Add(new Category
				{
					Id = ObjectId.GenerateNewId(),
					UserId = user.Id,
					Name = data.Name,
					Color = data.Color,
					Icon = data.Icon,
					CreatedAt = DateTime.UtcNow
				});
			}
		}

		if (categories.Any())
		{
			await collection.InsertManyAsync(categories);
		}

		Console.WriteLine($"✓ Created {categories.Count} categories");
	}
	
}
