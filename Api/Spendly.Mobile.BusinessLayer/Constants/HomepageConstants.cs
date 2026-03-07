// CHANGED_BY_AI: 2026-03-02 - Add report constants
// CHANGED_BY_AI: 2026-03-02 - Add homepage constants
// CHANGED_BY_AI: 2026-03-02 - Add finance constants
namespace Spendly.Mobile.BusinessLayer.Constants;

public static class Constants
{
    public static class Homepage
    {
        public const int AccountTopCount = 5;
        public const int CategoryTopCount = 6;
        public const int TrendMonths = 6;
    }

    public static class Reports
    {
        public const int TopChangingCategories = 4;
        public const int DefaultPageSize = 10;
        public const string TrendIncrease = "increase";
        public const string TrendDecrease = "decrease";
        public const string TrendNeutral = "neutral";
    }

    public static class Finance
    {
        public static class Sort
        {
            public const string Name = "name";
            public const string Amount = "amount";
            public const string Transactions = "transactions";
            public const string Date = "date";
            public const string Asc = "asc";
            public const string Desc = "desc";
        }
    }

    public static class User
    {
        public const int TrialDurationInDays = 7;
    }

    public static class Application
    {
        public const string ApplicationNAme = "Spendly";
    }
}
