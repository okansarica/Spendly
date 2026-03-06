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
		var banks = await CreateBanks(users);
		var (accounts, currencies) = await CreateAccounts(banks);
		var categories = await CreateCategories(users);
		var merchants = await CreateMerchants(categories);
		var normalizedTransactions = await CreateNormalizedTransactions(users, accounts, categories, merchants, banks);
		await CreateSummaryTables(normalizedTransactions, accounts);
	}
	private async static Task<List<Bank>> CreateBanks(List<User> users)
	{
		var banks = new List<Bank>();
		foreach (var user in users)
		{
			for (int i = 0; i < 2; i++)
			{
				var bank = new Bank
				{
					UserId = user.Id,
					Name = $"{user.Name} Bank {i}"
				};
				await _database.GetCollection<Bank>("Bank").InsertOneAsync(bank);
				banks.Add(bank);
			}
		}
		return banks;
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

	static async Task<(List<Account>, List<ObjectId>)> CreateAccounts(List<Bank> banks)
	{
		var collection = _database.GetCollection<Account>("Account");
		await collection.DeleteManyAsync(FilterDefinition<Account>.Empty);

		var currencyIds = new List<ObjectId> { ObjectId.GenerateNewId(), ObjectId.GenerateNewId() };
		var accounts = new List<Account>();
		var accountTypes = new[] { AccountType.Bank, AccountType.CreditCard, AccountType.Cash };

		foreach (var bank in banks)
		{
			for (int i = 0; i < 3; i++)
			{
				accounts.Add(new Account
				{
					Id = ObjectId.GenerateNewId(),
					BankId = bank.Id,
					Name = $"{accountTypes[i]} Account - {bank.Name}",
					Type = accountTypes[i],
					CurrencyId = currencyIds[0]
				});
			}
		}

		await collection.InsertManyAsync(accounts);
		Console.WriteLine($"✓ Created {accounts.Count} accounts");
		return (accounts, currencyIds);
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

	static async Task<List<Category>> CreateCategories(List<User> users)
	{
		var collection = _database.GetCollection<Category>("Category");
		await collection.DeleteManyAsync(FilterDefinition<Category>.Empty);

		var categories = new List<Category>();
		var categoryNames = new[] { "Food", "Transport", "Entertainment", "Shopping", "Health", "Bills" };

		foreach (var user in users)
		{
			foreach (var name in categoryNames)
			{
				categories.Add(new Category
				{
					Id = ObjectId.GenerateNewId(),
					UserId = user.Id,
					Name = name,
					ParentId = null
				});
			}
		}

		await collection.InsertManyAsync(categories);
		Console.WriteLine($"✓ Created {categories.Count} categories");
		return categories;
	}

	static async Task<List<Merchant>> CreateMerchants(List<Category> categories)
	{
		var collection = _database.GetCollection<Merchant>("Merchant");
		await collection.DeleteManyAsync(FilterDefinition<Merchant>.Empty);

		var merchants = new List<Merchant>();
		var merchantNames = new[] { "Walmart", "Amazon", "Starbucks", "Shell Gas", "Netflix", "Spotify", "McDonald's", "Target", "Best Buy", "Costco" };
		var random = new Random();

		var userCategories = categories.GroupBy(c => c.UserId).ToList();

		foreach (var userCategoryGroup in userCategories)
		{
			var userId = userCategoryGroup.Key;
			var userCats = userCategoryGroup.ToList();

			foreach (var merchantName in merchantNames)
			{
				var transactionCount = random.Next(5, 50);
				var transactionAmount = transactionCount * (decimal)random.Next(20, 200);

				merchants.Add(new Merchant
				{
					Id = ObjectId.GenerateNewId(),
					UserId = userId,
					Name = merchantName,
					CategoryId = userCats[random.Next(userCats.Count)].Id,
					TransactionCount = transactionCount,
					TransactionAmount = transactionAmount
				});
			}
		}

		await collection.InsertManyAsync(merchants);
		Console.WriteLine($"✓ Created {merchants.Count} merchants");

		// Update MerchantCount on categories based on created merchants
		var categoryCollection = _database.GetCollection<Category>("Category");
		var merchantCounts = merchants
			.GroupBy(m => m.CategoryId ?? ObjectId.Empty)
			.ToDictionary(g => g.Key, g => g.Count());

		var updates = new List<WriteModel<Category>>();
		foreach (var cat in categories)
		{
			var count = merchantCounts.TryGetValue(cat.Id, out var c) ? c : 0;
			cat.MerchantCount = count;
			var filter = Builders<Category>.Filter.Eq(x => x.Id, cat.Id);
			var update = Builders<Category>.Update.Set(x => x.MerchantCount, count);
			updates.Add(new UpdateOneModel<Category>(filter, update));
		}

		if (updates.Any())
		{
			await categoryCollection.BulkWriteAsync(updates);
			Console.WriteLine($"✓ Updated merchant counts for {updates.Count} categories");
		}

		return merchants;
	}

	static async Task<List<NormalizedTransaction>> CreateNormalizedTransactions(
		List<User> users, List<Account> accounts, List<Category> categories, List<Merchant> merchants, List<Bank> banks)
	{
		var collection = _database.GetCollection<NormalizedTransaction>("NormalizedTransaction");
		await collection.DeleteManyAsync(FilterDefinition<NormalizedTransaction>.Empty);

		var rawTxnCollection = _database.GetCollection<RawTransaction>("RawTransaction");
		await rawTxnCollection.DeleteManyAsync(FilterDefinition<RawTransaction>.Empty);

		var transactions = new List<NormalizedTransaction>();
		var rawTransactions = new List<RawTransaction>();
		var random = new Random();
		var amounts = new[] { 10m, 25m, 50m, 100m, 150m, 200m, 350m, 500m, 750m, 1000m };

		var startDate = DateTime.UtcNow.AddMonths(-6);
		var currentMonth = new DateTime(2026, 3, 1);
		var previousMonth = new DateTime(2026, 2, 1);

		foreach (var user in users)
		{
			var userBankIds = banks.Where(b => b.UserId == user.Id).Select(b => b.Id).ToList();
			var userAccounts = accounts.Where(a => userBankIds.Contains(a.BankId)).ToList();
			var userCategories = categories.Where(c => c.UserId == user.Id).ToList();
			var userMerchants = merchants.Where(m => m.UserId == user.Id).ToList();

			if (!userAccounts.Any())
			{
				continue;
			}

			for (int i = 0; i < 150; i++)
			{
				var txnDate = startDate.AddDays(random.Next(0, 180));
				var account = userAccounts[random.Next(userAccounts.Count)];
				var category = userCategories[random.Next(userCategories.Count)];
				var merchant = userMerchants[random.Next(userMerchants.Count)];
				var amount = amounts[random.Next(amounts.Length)];

				var rawTxn = new RawTransaction
				{
					Id = ObjectId.GenerateNewId(),
					UserId = user.Id,
					Amount = amount,
					MerchantName = merchant.Name,
					CreatedAt = DateTime.UtcNow
				};

				var normalizedTxn = new NormalizedTransaction
				{
					Id = ObjectId.GenerateNewId(),
					RawTransactionId = rawTxn.Id,
					UserId = user.Id,
					AccountId = account.Id,
					Date = txnDate,
					Amount = amount,
					CategoryId = category.Id,
					MerchantId = merchant.Id,
					CreatedAt = DateTime.UtcNow
				};

				rawTransactions.Add(rawTxn);
				transactions.Add(normalizedTxn);
			}

			for (int i = 0; i < 200; i++)
			{
				var txnDate = previousMonth.AddDays(random.Next(0, 28));
				var account = userAccounts[random.Next(userAccounts.Count)];
				var category = userCategories[random.Next(userCategories.Count)];
				var merchant = userMerchants[random.Next(userMerchants.Count)];
				var amount = amounts[random.Next(amounts.Length)];

				var rawTxn = new RawTransaction
				{
					Id = ObjectId.GenerateNewId(),
					UserId = user.Id,
					Amount = amount,
					MerchantName = merchant.Name,
					CreatedAt = DateTime.UtcNow
				};

				var normalizedTxn = new NormalizedTransaction
				{
					Id = ObjectId.GenerateNewId(),
					RawTransactionId = rawTxn.Id,
					UserId = user.Id,
					AccountId = account.Id,
					Date = txnDate,
					Amount = amount,
					CategoryId = category.Id,
					MerchantId = merchant.Id,
					CreatedAt = DateTime.UtcNow
				};

				rawTransactions.Add(rawTxn);
				transactions.Add(normalizedTxn);
			}

			for (int i = 0; i < 250; i++)
			{
				var txnDate = currentMonth.AddDays(random.Next(0, 2));
				var account = userAccounts[random.Next(userAccounts.Count)];
				var category = userCategories[random.Next(userCategories.Count)];
				var merchant = userMerchants[random.Next(userMerchants.Count)];
				var amount = amounts[random.Next(amounts.Length)];

				var rawTxn = new RawTransaction
				{
					Id = ObjectId.GenerateNewId(),
					UserId = user.Id,
					Amount = amount,
					MerchantName = merchant.Name,
					CreatedAt = DateTime.UtcNow
				};

				var normalizedTxn = new NormalizedTransaction
				{
					Id = ObjectId.GenerateNewId(),
					RawTransactionId = rawTxn.Id,
					UserId = user.Id,
					AccountId = account.Id,
					Date = txnDate,
					Amount = amount,
					CategoryId = category.Id,
					MerchantId = merchant.Id,
					CreatedAt = DateTime.UtcNow
				};

				rawTransactions.Add(rawTxn);
				transactions.Add(normalizedTxn);
			}
		}

		await collection.InsertManyAsync(transactions);
		await rawTxnCollection.InsertManyAsync(rawTransactions);
		Console.WriteLine($"✓ Created {transactions.Count} normalized transactions");
		return transactions;
	}

	static async Task CreateSummaryTables(List<NormalizedTransaction> transactions, List<Account> accounts)
	{
		await CreateDailyUserExpenses(transactions);
		await CreateDailyCategoryExpenses(transactions);
		await CreateDailyAccountExpenses(transactions, accounts);
		await CreateDailyCategoryAccountExpenses(transactions, accounts);
		await CreateMonthlyCategoryExpenses(transactions);
		await CreateMonthlyMerchantExpenses(transactions);
		await CreateMonthlyUserExpenses(transactions);
	}

	static async Task CreateDailyUserExpenses(List<NormalizedTransaction> transactions)
	{
		var collection = _database.GetCollection<DailyUserExpense>("DailyUserExpense");
		await collection.DeleteManyAsync(FilterDefinition<DailyUserExpense>.Empty);

		var grouped = transactions
			.GroupBy(t => new { t.UserId, Date = t.Date.Date })
			.Select(g => new DailyUserExpense
			{
				Id = ObjectId.GenerateNewId(),
				UserId = g.Key.UserId,
				DateTime = g.Key.Date,
				TotalAmount = g.Sum(t => t.Amount),
				CreatedAt = DateTime.UtcNow
			})
			.ToList();

		await collection.InsertManyAsync(grouped);
		Console.WriteLine($"✓ Created {grouped.Count} daily user expenses");
	}

	static async Task CreateDailyCategoryExpenses(List<NormalizedTransaction> transactions)
	{
		var collection = _database.GetCollection<DailyCategoryExpense>("DailyCategoryExpense");
		await collection.DeleteManyAsync(FilterDefinition<DailyCategoryExpense>.Empty);

		var grouped = transactions
			.GroupBy(t => new { t.UserId, t.CategoryId, Date = t.Date.Date })
			.Select(g => new DailyCategoryExpense
			{
				Id = ObjectId.GenerateNewId(),
				UserId = g.Key.UserId,
				CategoryId = g.Key.CategoryId ?? ObjectId.Empty,
				DateTime = g.Key.Date,
				TotalAmount = g.Sum(t => t.Amount),
				CreatedAt = DateTime.UtcNow
			})
			.ToList();

		await collection.InsertManyAsync(grouped);
		Console.WriteLine($"✓ Created {grouped.Count} daily category expenses");
	}

	static async Task CreateDailyAccountExpenses(List<NormalizedTransaction> transactions, List<Account> accounts)
	{
		var collection = _database.GetCollection<DailyAccountExpense>("DailyAccountExpense");
		await collection.DeleteManyAsync(FilterDefinition<DailyAccountExpense>.Empty);

		var accountBankMap = accounts.ToDictionary(a => a.Id, a => a.BankId);

		var grouped = transactions
			.GroupBy(t => new { t.UserId, t.AccountId, Date = t.Date.Date })
			.Select(g => new DailyAccountExpense
			{
				Id = ObjectId.GenerateNewId(),
				UserId = g.Key.UserId,
				DateTime = g.Key.Date,
				BankId = accountBankMap.TryGetValue(g.Key.AccountId, out var bId) ? bId : ObjectId.Empty,
				AccountId = g.Key.AccountId,
				TotalAmount = g.Sum(t => t.Amount),
				CreatedAt = DateTime.UtcNow
			})
			.ToList();

		await collection.InsertManyAsync(grouped);
		Console.WriteLine($"✓ Created {grouped.Count} daily account expenses");
	}

	static async Task CreateDailyCategoryAccountExpenses(List<NormalizedTransaction> transactions, List<Account> accounts)
	{
		var collection = _database.GetCollection<DailyCategoryAccountExpense>("DailyCategoryAccountExpense");
		await collection.DeleteManyAsync(FilterDefinition<DailyCategoryAccountExpense>.Empty);

		var accountBankMap = accounts.ToDictionary(a => a.Id, a => a.BankId);

		var grouped = transactions
			.GroupBy(t => new { t.UserId, t.CategoryId, t.AccountId, Date = t.Date.Date })
			.Select(g => new DailyCategoryAccountExpense
			{
				Id = ObjectId.GenerateNewId(),
				UserId = g.Key.UserId,
				CategoryId = g.Key.CategoryId ?? ObjectId.Empty,
				BankId = accountBankMap.TryGetValue(g.Key.AccountId, out var bId) ? bId : ObjectId.Empty,
				AccountId = g.Key.AccountId,
				Date = g.Key.Date,
				TotalAmount = g.Sum(t => t.Amount),
				CreatedAt = DateTime.UtcNow
			})
			.ToList();

		await collection.InsertManyAsync(grouped);
		Console.WriteLine($"✓ Created {grouped.Count} daily category account expenses");
	}

	static async Task CreateMonthlyCategoryExpenses(List<NormalizedTransaction> transactions)
	{
		var collection = _database.GetCollection<CategoryMonthlyExpense>("CategoryMonthlyExpense");
		await collection.DeleteManyAsync(FilterDefinition<CategoryMonthlyExpense>.Empty);

		var grouped = transactions
			.GroupBy(t => new { t.CategoryId, Year = t.Date.Year, Month = t.Date.Month })
			.Select(g => new CategoryMonthlyExpense
			{
				Id = ObjectId.GenerateNewId(),
				CategoryId = g.Key.CategoryId ?? ObjectId.Empty,
				Year = g.Key.Year,
				Month = g.Key.Month,
				TransactionCount = g.Count(),
				TransactionAmount = g.Sum(t => t.Amount),
				CreatedAt = DateTime.UtcNow
			})
			.ToList();

		await collection.InsertManyAsync(grouped);
		Console.WriteLine($"✓ Created {grouped.Count} monthly category expenses");
	}

	static async Task CreateMonthlyMerchantExpenses(List<NormalizedTransaction> transactions)
	{
		var collection = _database.GetCollection<MerchantMonthlyExpense>("MerchantMonthlyExpense");
		await collection.DeleteManyAsync(FilterDefinition<MerchantMonthlyExpense>.Empty);

		var grouped = transactions
			.GroupBy(t => new { t.MerchantId, Year = t.Date.Year, Month = t.Date.Month })
			.Select(g => new MerchantMonthlyExpense
			{
				Id = ObjectId.GenerateNewId(),
				MerchantId = g.Key.MerchantId,
				Year = g.Key.Year,
				Month = g.Key.Month,
				TransactionCount = g.Count(),
				TransactionAmount = g.Sum(t => t.Amount),
				CreatedAt = DateTime.UtcNow
			})
			.ToList();

		await collection.InsertManyAsync(grouped);
		Console.WriteLine($"✓ Created {grouped.Count} monthly merchant expenses");
	}

	static async Task CreateMonthlyUserExpenses(List<NormalizedTransaction> transactions)
	{
		var collection = _database.GetCollection<MonthlyUserExpense>("MonthlyUserExpense");
		await collection.DeleteManyAsync(FilterDefinition<MonthlyUserExpense>.Empty);

		var grouped = transactions
			.GroupBy(t => new { t.UserId, Year = t.Date.Year, Month = t.Date.Month })
			.Select(g => new MonthlyUserExpense
			{
				Id = ObjectId.GenerateNewId(),
				UserId = g.Key.UserId,
				Year = g.Key.Year,
				Month = g.Key.Month,
				TotalAmount = g.Sum(t => t.Amount),
				CreatedAt = DateTime.UtcNow
			})
			.ToList();

		await collection.InsertManyAsync(grouped);
		Console.WriteLine($"✓ Created {grouped.Count} monthly user expenses");
	}
}
