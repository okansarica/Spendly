// CHANGED_BY_AI: 2026-02-28 - Added LoginProviderType enum for auth
namespace Spendly.Shared.Enums;

public enum LoginProviderType
{
    Local,
    Google,
    Facebook
}

public enum UserSubscriptionDurationType
{
    Monthly,
    Yearly
}

public enum UserSubscriptionPaymentStatusType
{
    WaitingForPayment,
    Paid,
}

public enum SubscriptionType
{
    Trial,
    Paid,
}
