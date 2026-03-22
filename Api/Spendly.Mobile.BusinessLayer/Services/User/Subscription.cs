namespace Spendly.Mobile.BusinessLayer.Services.User;

using Shared.Enums;

public interface ISubscription
{
	SubscriptionType Type { get; }
	string Name { get; }
	decimal MonthlyPrice { get; }
	decimal YearlyPrice { get; }
	public int? TransactionLimit { get;  }
	public bool CanUseOpenBanking { get;  }
}

public sealed class FreeSubscription : ISubscription
{

	public SubscriptionType Type => SubscriptionType.Free;
	public string Name  => "Free";
	public decimal MonthlyPrice => 0;
	public decimal YearlyPrice => 0;
	public int? TransactionLimit => 100;
	public bool CanUseOpenBanking => false;
}


public sealed class PlusSubscription : ISubscription
{

	public SubscriptionType Type => SubscriptionType.Free;
	public string Name  => "Plus";
	public decimal MonthlyPrice => 3.99m;
	public decimal YearlyPrice => 39.99m;
	public int? TransactionLimit => null;
	public bool CanUseOpenBanking => false;
}


public sealed class ProSubscription : ISubscription
{

	public SubscriptionType Type => SubscriptionType.Free;
	public string Name  => "Pro";
	public decimal MonthlyPrice => 6.99m;
	public decimal YearlyPrice => 69.99m;
	public int? TransactionLimit => 100;
	public bool CanUseOpenBanking => true;
}
