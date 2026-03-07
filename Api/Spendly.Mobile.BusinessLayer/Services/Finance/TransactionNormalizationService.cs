// namespace Spendly.Mobile.BusinessLayer.Services.Finance;
//
// using Microsoft.Extensions.Logging;
// using MongoDB.Bson;
// using Spendly.Shared.DataLayer;
// using Spendly.Shared.Entities.TransactionManagement;
// using Spendly.Shared.Entities.UserManagement;
//
// public class TransactionNormalizationService(
//     IRepository<RawTransaction> rawTransactionRepository,
//     IRepository<NormalizedTransaction> normalizedTransactionRepository,
//     IRepository<Account> accountRepository,
//     IRepository<Merchant> merchantRepository,
//     ILogger<TransactionNormalizationService> logger)
// {
//     public async Task NormalizeTransactionsAsync(List<ObjectId> rawTransactionIds)
//     {
//         logger.LogInformation("Starting normalization for {Count} raw transactions", rawTransactionIds.Count);
//
//         var rawTransactions = await rawTransactionRepository.ListAsync(rawTransactionIds);
//
//         logger.LogInformation("Retrieved {Count} raw transactions for normalization", rawTransactions.Count);
//
//         var normalizedCount = 0;
//         var skippedCount = 0;
//
//         foreach (var rawTransaction in rawTransactions.Values)
//         {
//             var existingNormalized = await normalizedTransactionRepository.GetAsync(x => x.RawTransactionId == rawTransaction.Id);
//
//             if (existingNormalized != null)
//             {
//                 skippedCount++;
//                 logger.LogDebug("Skipping already normalized transaction {RawTransactionId}", rawTransaction.Id);
//                 continue;
//             }
//
//             var account = await accountRepository.GetRequiredAsync(x => x.PlaidAccountId == rawTransaction.PlaidAccountId);
//             
//             var merchant = await GetOrCreateMerchantAsync(rawTransaction);
//
//             var normalizedTransaction = new NormalizedTransaction
//             {
//                 RawTransactionId = rawTransaction.Id,
//                 UserId = rawTransaction.UserId,
//                 AccountId = account.Id,
//                 Date = rawTransaction.DateTime,
//                 Amount = rawTransaction.Amount,
//                 CategoryId = merchant.CategoryId,
//                 MerchantId = merchant.Id
//             };
//
//             await normalizedTransactionRepository.InsertAsync(normalizedTransaction);
//             normalizedCount++;
//
//             logger.LogDebug("Normalized transaction {RawTransactionId} to {NormalizedTransactionId}",
//                 rawTransaction.Id, normalizedTransaction.Id);
//         }
//
//         logger.LogInformation("Completed normalization. Normalized: {NormalizedCount}, Skipped: {SkippedCount}",
//             normalizedCount, skippedCount);
//     }
//
//     private async Task<Merchant> GetOrCreateMerchantAsync(RawTransaction rawTransaction)
//     {
//         var existingMerchant = await merchantRepository.GetAsync(x =>
//             x.UserId == rawTransaction.UserId &&
//             x.Name == rawTransaction.MerchantName);
//
//         if (existingMerchant != null)
//         {
//             existingMerchant.TransactionCount++;
//             existingMerchant.TransactionAmount += rawTransaction.Amount;
//             await merchantRepository.UpdateAsync(existingMerchant);
//
//             logger.LogDebug("Updated existing merchant {MerchantId} with new transaction", existingMerchant.Id);
//             return existingMerchant;
//         }
//
//         var newMerchant = new Merchant
//         {
//             UserId = rawTransaction.UserId,
//             Name = rawTransaction.MerchantName,
//             CategoryId = null,
//             TransactionCount = 1,
//             TransactionAmount = rawTransaction.Amount
//         };
//
//         await merchantRepository.InsertAsync(newMerchant);
//
//         logger.LogInformation("Created new merchant {MerchantId} for name {MerchantName}",
//             newMerchant.Id, newMerchant.Name);
//
//         return newMerchant;
//     }
// }
//
