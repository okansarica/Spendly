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
		await CreateBankDefinitions();
		await CreateUserSubscriptions(users);
		var allCategories = await CreateCategories(users);
		await CreatePreDefinedMerchants(allCategories); 
		var userCategories = await CreateUserCategories(users);
		await CreateMerchants(allCategories.Single(p => p.IsOther).Id, users, userCategories);
	}
	private async static Task CreatePreDefinedMerchants(List<Category> allCategories)
	{
		var collection = _database.GetCollection<PredefinedMerchant>("PredefinedMerchant");

		var merchantNames = new[]
		{
			
				"Adidas",
				"Aldi",
				"Alfies Fish & Chips",
				"Am 2 Pm Convenience Store",
				"Amazon Prime Video",
				"And Customs",
				"Apple Store",
				"Blue Planet Aquarium",
				"Brighouse Swimming Pool & Fitness Centre",
				"Budget Insurance",
				"Bulb Energy",
				"Bupa",
				"Burberry",
				"Cambridge City Council",
				"Capital One",
				"Cartridge Discount",
				"Ceramic Tile Company",
				"CHIP",
				"Diamond Insurance",
				"Elemis Spa",
				"First Direct Arena",
				"Five Rivers Coffee Company",
				"Flixbus",
				"H&M",
				"Harrods",
				"HelloFresh",
				"Home Bargains",
				"Homeserve",
				"IKEA",
				"King Street Brew House",
				"Kingshill Cars",
				"Lloyds Pharmacy",
				"Loans 2 Go",
				"Manchester Arena",
				"Marks & Spencer",
				"Microsoft",
				"MotorSport Vision",
				"Nail Co",
				"Nando's",
				"Netflix",
				"Non-Sterling Transaction Fee NON-STERLING TRANSACTION",
				"Northenden Golf Club",
				"Nottinghill Pharmacy",
				"Park Veterinary Centre",
				"Pets And Claws Pet Insurance",
				"Planet Spice",
				"Pokerstars",
				"Pret A Manger",
				"Primark",
				"Rookery Mini Market",
				"Royal Mail",
				"Ryanair",
				"Sainsbury's",
				"Shop",
				"Sky",
				"Sports",
				"Sports Direct",
				"Spotify",
				"Star Food And Wine",
				"Starbucks",
				"Stobswell Dental Practice",
				"Takeway",
				"Tesco",
				"The Zoological Society of London",
				"Tradingview",
				"Trainline",
				"Uber",
				"UniBet",
				"VITALITY HEALTH",
				"Wage Day Advance",
				"Walmart",
				"Wilko",
				"YouLend"
			
		};

		var rand = new Random();
		var merchants = new List<PredefinedMerchant>();
		foreach (var name in merchantNames)
		{
			var category = allCategories[rand.Next(allCategories.Count)];
			merchants.Add(new PredefinedMerchant
			{
				Name = name,
				CategoryId = category.Id,
				PlaidId = null
			});
		}

		if (merchants.Any())
		{
			await collection.InsertManyAsync(merchants);
		}

		Console.WriteLine($"✓ Created {merchants.Count} predefined merchants");
	}



	static async Task<List<User>> CreateUsers()
	{
		var collection = _database.GetCollection<User>("User");
		await collection.DeleteManyAsync(FilterDefinition<User>.Empty);

		var passwordHash = HashPassword("123");

		var users = new List<User>
		{
			// new User
			// {
			// 	Id = ObjectId.GenerateNewId(),
			// 	Name = "Ahmet",
			// 	Surname = "Yilmaz",
			// 	Email = "ahmet@example.com",
			// 	PasswordHash = passwordHash,
			// 	IsActive = true,
			// 	EmailVerification = new EmailVerification {IsVerified = true},
			// 	LoginProviders =
			// 	[
			// 		new UserLoginProvider
			// 		{
			// 			Provider = LoginProviderType.Local,
			// 		}
			// 	]
			// },
			// new User
			// {
			// 	Id = ObjectId.GenerateNewId(),
			// 	Name = "Fatih",
			// 	Surname = "Kaya",
			// 	Email = "fatih@example.com",
			// 	PasswordHash = passwordHash,
			// 	IsActive = true,
			// 	EmailVerification = new EmailVerification {IsVerified = true}
			// },
			// new User
			// {
			// 	Id = ObjectId.GenerateNewId(),
			// 	Name = "Zeynep",
			// 	Surname = "Demir",
			// 	Email = "zeynep@example.com",
			// 	PasswordHash = passwordHash,
			// 	IsActive = true,
			// 	EmailVerification = new EmailVerification {IsVerified = true},
			// 	LoginProviders =
			// 	[
			// 		new UserLoginProvider
			// 		{
			// 			Provider = LoginProviderType.Local,
			// 		}
			// 	]
			// },
			new User
			{
				Id = ObjectId.GenerateNewId(),
				Name = "Okan",
				Surname = "Sarica",
				Email = "okansarica@gmail.com",
				PasswordHash = passwordHash,
				IsActive = true,
				EmailVerification = new EmailVerification {IsVerified = true},
				LoginProviders =
				[
					new UserLoginProvider
					{
						Provider = LoginProviderType.Local,
					}
				]
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
				SubscriptionType = SubscriptionType.Trial,
				CreatedAt = DateTime.UtcNow,
				State =  UserSubscriptionStateType.Active,
			
			});
		}

		if (subscriptions.Any())
		{
			await collection.InsertManyAsync(subscriptions);
		}

		Console.WriteLine($"✓ Created {subscriptions.Count} trial subscriptions");
	}

	static async Task<List<Category>> CreateCategories(List<User> users)
	{
		var collection = _database.GetCollection<Category>("Category");
		await collection.DeleteManyAsync(FilterDefinition<Category>.Empty);

		var categories = new List<Category>();
		var categoryData = new[]
		{
			new {Name = "Food & Dining", Color = "#FF6B6B", Icon = "🍔"},
			new {Name = "Transportation", Color = "#4ECDC4", Icon = "🚗"},
			new {Name = "Shopping", Color = "#45B7D1", Icon = "🛍️"},
			new {Name = "Entertainment", Color = "#FFA07A", Icon = "🎬"},
			new {Name = "Bills & Utilities", Color = "#98D8C8", Icon = "💡"},
			new {Name = "Groceries", Color = "#98D8C8", Icon = "💡"},
			new {Name = "Health", Color = "#98D8C8", Icon = "💡"},
			new {Name = "Housing", Color = "#98D8C8", Icon = "💡"},
			new {Name = "Other", Color = "#98D8C8", Icon = "💡"},
		};

		foreach (var data in categoryData)
		{
			categories.Add(new Category
			{
				Id = ObjectId.GenerateNewId(),
				Name = data.Name,
				Color = data.Color,
				Icon = null,
				CreatedAt = DateTime.UtcNow,
				IsOther = data.Name == "Other"
			});
		}


		if (categories.Any())
		{
			await collection.InsertManyAsync(categories);
		}

		Console.WriteLine($"✓ Created {categories.Count} categories");

		return categories;
	}

	static async Task<List<UserCategory>> CreateUserCategories(List<User> users)
	{
		var categoryCollection = _database.GetCollection<Category>("Category");
		var userCategoryCollection = _database.GetCollection<UserCategory>("UserCategory");
		await userCategoryCollection.DeleteManyAsync(FilterDefinition<UserCategory>.Empty);

		var predefinedCategories = await categoryCollection.Find(FilterDefinition<Category>.Empty).ToListAsync();

		var userCategories = new List<UserCategory>();
		foreach (var user in users)
		{
			foreach (var category in predefinedCategories)
			{
				userCategories.Add(new UserCategory
				{
					UserId = user.Id,
					ParentId = null,
					CategoryId = category.Id,
					Name = category.Name,
					Color = category.Color,
					Icon = category.Icon,
					MerchantCount = 0,
					IsOther = category.IsOther
				});
			}
		}

		if (userCategories.Any())
		{
			await userCategoryCollection.InsertManyAsync(userCategories);
		}

		Console.WriteLine($"✓ Created {userCategories.Count} user categories");

		return userCategories;
	}

	static async Task CreateMerchants(ObjectId otherCategoryId, List<User> users, List<UserCategory> userCategories)
	{
		var collection = _database.GetCollection<Merchant>("Merchant");

		// Ensure the special 'Other' merchant exists instead of wiping the collection
		var otherMerchant = await collection.Find(m => m.IsOther).FirstOrDefaultAsync();
		if (otherMerchant == null)
		{
			otherMerchant = new Merchant
			{
				Id = ObjectId.GenerateNewId(),
				Name = "Other",
				IsOther = true,
				CreatedAt = DateTime.UtcNow,
				CategoryId = otherCategoryId
			};
			await collection.InsertOneAsync(otherMerchant);
		}

		var merchants = new List<Merchant> { otherMerchant };

		var userMerchantsCollection = _database.GetCollection<UserMerchant>("UserMerchant");
		await userMerchantsCollection.DeleteManyAsync(FilterDefinition<UserMerchant>.Empty);

		foreach (var user in users)
		{
			await userMerchantsCollection.InsertManyAsync(new[]
			{
				new UserMerchant
				{
					IsOther = true,
					MerchantId = otherMerchant.Id,
					TotalTransactionAmount = 0,
					TotalTransactionCount = 0,
					UserCategoryId = userCategories.Single(p => p.UserId == user.Id && p.IsOther).Id,
					UserId = user.Id
				}
			});
		}

		Console.WriteLine($"✓ Created {merchants.Count} merchants");
	}

	private static async Task CreateBankDefinitions()
	{
		var collection = _database.GetCollection<BankDefinition>("BankDefinition");
		await collection.DeleteManyAsync(FilterDefinition<BankDefinition>.Empty);

		var bankNames = new[]
		{
			"Barclays",
			"HSBC",
			"Lloyds Bank",
			"NatWest",
			"Royal Bank of Scotland",
			"Santander UK",
			"Halifax",
			"Nationwide",
			"TSB",
			"Metro Bank",
			"First Direct",
			"Monzo",
			"Starling Bank",
			"Revolut",
			"Virgin Money"
		};

		var banks = new List<BankDefinition>();
		foreach (var name in bankNames)
		{
			banks.Add(new BankDefinition
			{
				Id = ObjectId.GenerateNewId(),
				Name = name,
				CreatedAt = DateTime.UtcNow
			});
		}

		if (banks.Any())
		{
			await collection.InsertManyAsync(banks);
		}

		Console.WriteLine($"✓ Created {banks.Count} bank definitions");
	}

}
