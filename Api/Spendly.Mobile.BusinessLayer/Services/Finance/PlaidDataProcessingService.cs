// namespace Spendly.Mobile.BusinessLayer.Services.Finance;
//
// using Microsoft.AspNetCore.DataProtection;
// using Microsoft.Extensions.Logging;
// using MongoDB.Bson;
// using Shared.ViewModels.Plaid;
// using Spendly.Shared.Core;
// using Spendly.Shared.DataLayer;
// using Spendly.Shared.Entities.Banking;
// using Spendly.Shared.Entities.TransactionManagement;
// using Spendly.Shared.Entities.UserManagement;
//
// public class PlaidDataProcessingService(
//     IRepository<Account> accountRepository,
//     IRepository<RawTransaction> rawTransactionRepository,
//     IRepository<UserPlaidToken> userPlaidTokenRepository,
//     IDataProtectionProvider dataProtectionProvider,
//     ILogger<PlaidDataProcessingService> logger)
// {
//     public async Task<List<ObjectId>> ProcessUserTransactionsAsync(string userId, Func<string, DateTime, DateTime, string, Task<List<PlaidTransaction>>> getTransactionsFunc)
//     {
//         logger.LogInformation("Fetching user plaid token for user {UserId}", userId);
//
//         var userPlaidToken = await userPlaidTokenRepository.GetRequiredAsync(x => x.UserId == userId);
//
//         var protector = dataProtectionProvider.CreateProtector("UserPlaidTokenProtector");
//         var accessToken = protector.Unprotect(userPlaidToken.EncryptedAccessToken);
//
//         logger.LogInformation("Fetching accounts for user {UserId}", userId);
//
//         var accounts = await accountRepository.ListAsync(x => x.PlaidAccountId != null && !x.IsDeleted);
//         var userAccounts = accounts.Where(a => !string.IsNullOrEmpty(a.PlaidAccountId)).ToList();
//
//         logger.LogInformation("Found {Count} accounts for user {UserId}", userAccounts.Count, userId);
//
//         var endDate = DateTime.UtcNow;
//         var startDate = endDate.AddDays(-90);
//
//         logger.LogInformation("Fetching transactions from {StartDate} to {EndDate} for user {UserId}", startDate, endDate, userId);
//
//         var transactions = await getTransactionsFunc(accessToken, startDate, endDate, userId);
//
//         logger.LogInformation("Retrieved {Count} transactions from Plaid for user {UserId}", transactions.Count, userId);
//
//         var savedCount = 0;
//         var skippedCount = 0;
//         var savedRawTransactionIds = new List<ObjectId>();
//
//         foreach (var transaction in transactions)
//         {
//             var existingTransaction = await rawTransactionRepository.GetAsync(x => x.PlaidTransactionId == transaction.TransactionId);
//
//             if (existingTransaction != null)
//             {
//                 skippedCount++;
//                 logger.LogDebug("Skipping duplicate transaction {TransactionId} for user {UserId}", transaction.TransactionId, userId);
//                 continue;
//             }
//
//             var rawTransaction = new RawTransaction
//             {
//                 UserId = userId.ToObjectId(),
//                 DateTime = transaction.Date,
//                 Amount = transaction.Amount,
//                 MerchantName = transaction.MerchantName,
//                 RawMerchant = transaction.Name,
//                 PlaidAccountId = transaction.AccountId,
//                 PlaidTransactionId = transaction.TransactionId
//             };
//
//             await rawTransactionRepository.InsertAsync(rawTransaction);
//             savedRawTransactionIds.Add(rawTransaction.Id);
//             savedCount++;
//
//             logger.LogDebug("Saved transaction {TransactionId} for user {UserId}, Amount: {Amount}, Merchant: {Merchant}",
//                 transaction.TransactionId, userId, transaction.Amount, transaction.MerchantName);
//         }
//
//         logger.LogInformation("Completed processing transactions for user {UserId}. Saved: {SavedCount}, Skipped: {SkippedCount}",
//             userId, savedCount, skippedCount);
//             
//         return savedRawTransactionIds;
//     }
// }
