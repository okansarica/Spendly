// CHANGED_BY_AI: 2026-03-03 - Add user profile, password, language, and account deletion service
namespace Spendly.Mobile.BusinessLayer.Services.User;

using Shared.BusinessLayer;
using Shared.Enums;
using Spendly.Shared.Core.Interception;
using Spendly.Mobile.ViewModels.User;
using Spendly.Shared.Core;
using Spendly.Shared.DataLayer;
using Spendly.Shared.Entities.Auth;
using Spendly.Shared.Entities.LocaleManagement;
using Spendly.Shared.Entities.Reporting;
using Spendly.Shared.Entities.TransactionManagement;
using Spendly.Shared.Entities.UserManagement;
using Spendly.Shared.Entities.Subscription;
using Spendly.Shared.Localization;
using Spendly.Shared.ViewModels;

public class UserService(
    IRepository<User> userRepository,
    IRepository<UserRefreshToken> refreshTokenRepository,
    IRepository<RawTransaction> rawTransactionRepository,
    IRepository<NormalizedTransaction> normalizedTransactionRepository,
    IRepository<UserCategory> categoryRepository,
    IRepository<UserMerchant> merchantRepository,
    IRepository<Account> accountRepository,
    IRepository<DailyCategoryExpense> dailyCategoryExpenseRepository,
    IRepository<DailyAccountExpense> dailyAccountExpenseRepository,
    IRepository<DailyCategoryAccountExpense> dailyCategoryAccountExpenseRepository,
    IRepository<DailyUserExpense> dailyUserExpenseRepository,
    IRepository<MonthlyUserExpense> monthlyUserExpenseRepository,
    IRepository<CategoryMonthlyExpense> categoryMonthlyExpenseRepository,
    IRepository<MerchantMonthlyExpense> merchantMonthlyExpenseRepository,
    IRepository<UserSubscription> userSubscriptionRepository,
    IRepository<Bank> bankRepository,
    EmailService emailService,
    RequestContextViewModel requestContextViewModel)
{
    public async Task<FunctionResponse<UserProfileResponseViewModel>> GetProfileAsync()
    {
        var user = await userRepository.GetRequiredAsync(requestContextViewModel.UserId.ToObjectId());
        return FunctionResponse.Success(ToProfile(user));
    }

    public async Task<FunctionResponse<UserProfileResponseViewModel>> UpdateProfileAsync(UpdateUserProfileRequestViewModel request)
    {
        var user = await userRepository.GetRequiredAsync(requestContextViewModel.UserId.ToObjectId());
        user.Name = request.Name;
        user.Surname = request.Surname;
        user.IsNewsletterSubscribed = request.IsNewsletterSubscribed;
        user.UpdatedAt = DateTime.UtcNow;
        await userRepository.UpdateAsync(user);
        return FunctionResponse.Success(ToProfile(user));
    }

    public async Task<FunctionResponse> ChangePasswordAsync(ChangePasswordRequestViewModel request)
    {
        var user = await userRepository.GetRequiredAsync(requestContextViewModel.UserId.ToObjectId());
        if (string.IsNullOrWhiteSpace(user.PasswordHash) || !PasswordHelper.VerifyPassword(request.CurrentPassword, user.PasswordHash))
        {
            return FunctionResponse.Failure(MessageCodes.InvalidCredentials);
        }

        if (request.NewPassword != request.ConfirmNewPassword)
        {
            return FunctionResponse.Failure(MessageCodes.PasswordRequired);
        }

        user.PasswordHash = PasswordHelper.HashPassword(request.NewPassword);
        await userRepository.UpdateAsync(user);
        return FunctionResponse.Success();
    }

    public async Task<FunctionResponse<LanguagePreferenceResponseViewModel>> SetLanguagePreferenceAsync(SetLanguagePreferenceRequestViewModel request)
    {
        var normalizedCode = request.LanguageCode.Trim().ToLowerInvariant();
        if (!LocaleData.Languages.Any(x => x.Code.Equals(normalizedCode, StringComparison.OrdinalIgnoreCase)))
        {
            throw new Exception($"Invalid language code. UserId: {requestContextViewModel.UserId} code: {request.LanguageCode} code: {normalizedCode}");
        }

        var user = await userRepository.GetRequiredAsync(requestContextViewModel.UserId.ToObjectId());
        user.LanguageCode = normalizedCode;
        await userRepository.UpdateAsync(user);

        return FunctionResponse.Success(new LanguagePreferenceResponseViewModel
        {
            LanguageCode = user.LanguageCode,
        });
    }

    public async Task<FunctionResponse> DeleteAccountAsync()
    {
        var userId = requestContextViewModel.UserId.ToObjectId();
        var user = await userRepository.GetRequiredAsync(userId);

        var categories = await categoryRepository.ListAsync(x => x.UserId == userId);
        var merchants = await merchantRepository.ListAsync(x => x.UserId == userId);

        foreach (var transaction in await normalizedTransactionRepository.ListAsync(x => x.UserId == userId))
        {
            await normalizedTransactionRepository.DeleteAsync(transaction.Id);
        }

        foreach (var rawTransaction in await rawTransactionRepository.ListAsync(x => x.UserId == userId))
        {
            await rawTransactionRepository.DeleteAsync(rawTransaction.Id);
        }

        foreach (var dailyCategoryExpense in await dailyCategoryExpenseRepository.ListAsync(x => x.UserId == userId))
        {
            await dailyCategoryExpenseRepository.DeleteAsync(dailyCategoryExpense.Id);
        }

        foreach (var dailyAccountExpense in await dailyAccountExpenseRepository.ListAsync(x => x.UserId == userId))
        {
            await dailyAccountExpenseRepository.DeleteAsync(dailyAccountExpense.Id);
        }

        foreach (var dailyCategoryAccountExpense in await dailyCategoryAccountExpenseRepository.ListAsync(x => x.UserId == userId))
        {
            await dailyCategoryAccountExpenseRepository.DeleteAsync(dailyCategoryAccountExpense.Id);
        }

        foreach (var dailyUserExpense in await dailyUserExpenseRepository.ListAsync(x => x.UserId == userId))
        {
            await dailyUserExpenseRepository.DeleteAsync(dailyUserExpense.Id);
        }

        foreach (var monthlyUserExpense in await monthlyUserExpenseRepository.ListAsync(x => x.UserId == userId))
        {
            await monthlyUserExpenseRepository.DeleteAsync(monthlyUserExpense.Id);
        }

        var categoryIds = categories.Select(x => x.Id).ToHashSet();
        foreach (var categoryMonthlyExpense in await categoryMonthlyExpenseRepository.ListAsync(x => categoryIds.Contains(x.CategoryId)))
        {
            await categoryMonthlyExpenseRepository.DeleteAsync(categoryMonthlyExpense.Id);
        }

        var merchantIds = merchants.Select(x => x.Id).ToHashSet();
        foreach (var merchantMonthlyExpense in await merchantMonthlyExpenseRepository.ListAsync(x => merchantIds.Contains(x.MerchantId)))
        {
            await merchantMonthlyExpenseRepository.DeleteAsync(merchantMonthlyExpense.Id);
        }

        foreach (var bank in await bankRepository.ListAsync(p=>p.UserId == userId))
        {
            foreach (var account in await accountRepository.ListAsync(x => x.BankId == bank.Id))
            {
                await accountRepository.DeleteAsync(account.Id);
            }
            await bankRepository.DeleteAsync(bank.Id);
        }

        foreach (var category in categories)
        {
            await categoryRepository.DeleteAsync(category.Id);
        }

        foreach (var merchant in merchants)
        {
            await merchantRepository.DeleteAsync(merchant.Id);
        }

        foreach (var token in await refreshTokenRepository.ListAsync(x => x.UserId == userId))
        {
            await refreshTokenRepository.DeleteAsync(token.Id);
        }

        await userRepository.DeleteAsync(user.Id);
        return FunctionResponse.Success();
    }

    [Cacheable(DurationSeconds = 7200)]
    public async Task<FunctionResponse<DateTime?>> GetSubscriptionEndDateAsync()
    {
        var userId = requestContextViewModel.UserId.ToObjectId();
        var userSubscriptions = await userSubscriptionRepository.ListAsync(p => p.UserId == userId).ConfigureAwait(false);

        var paidActiveSubscriptions = userSubscriptions.Where(p =>
            p.SubscriptionType == SubscriptionType.Paid &&
            p.State == UserSubscriptionStateType.Active &&
            p.StartDateTime.HasValue &&
            p.StartDateTime.Value <= DateTime.UtcNow &&
            ((!p.EndDateTime.HasValue && p.ExpectedEndDateTime > DateTime.UtcNow) || (p.EndDateTime.HasValue && p.ExpectedEndDateTime > DateTime.UtcNow))).ToList();

        if (paidActiveSubscriptions.Count() > 1)
        {
            await emailService.SendAlarmEmailAsync($"User has multiple paid subscriptions. UserId: {requestContextViewModel.UserId}");
        }

        var paidActiveSubscription = paidActiveSubscriptions.FirstOrDefault();

        DateTime? subscriptionEndDate = null;
        if (paidActiveSubscription == null)
        {
            var trialSubscriptions = userSubscriptions.SingleOrDefault(p => p.SubscriptionType == SubscriptionType.Trial);
            subscriptionEndDate = trialSubscriptions?.EndDateTime ?? trialSubscriptions?.ExpectedEndDateTime;
        }

        return FunctionResponse.Success(subscriptionEndDate);
    }

    private static UserProfileResponseViewModel ToProfile(User user)
    {
        return new UserProfileResponseViewModel
        {
            Id = user.Id.ToString(),
            Name = user.Name,
            Surname = user.Surname,
            Email = user.Email,
            IsNewsletterSubscribed = user.IsNewsletterSubscribed,
            LanguageCode = user.LanguageCode,
        };
    }
}
