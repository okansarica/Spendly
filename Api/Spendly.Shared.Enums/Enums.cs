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
    Yearly,
}

public enum UserSubscriptionPaymentStatusType
{
    Waiting,
    Paid,
    Failed,
    Cancelled,
}

public enum SubscriptionPaymentResultStatusType
{
    Success,
    Fail,
    Exception,
}

public enum UserSubscriptionStateType
{
    Waiting,
    Active,
}
public enum SubscriptionType
{
    Free,
    Plus,
    Pro
}
