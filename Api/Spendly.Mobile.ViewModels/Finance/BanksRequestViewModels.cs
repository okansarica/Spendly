// CHANGED_BY_AI: 2026-03-06 - Add bank and account request view models
namespace Spendly.Mobile.ViewModels.Finance;

public class BankListRequestViewModel
{
    public string? Search { get; set; }
}

public class BankUpsertRequestViewModel
{
    public string? Name { get; set; }
    public string? BankDefinitionId { get; set; }
    public string? Description { get; set; }
}

public class BankAccountUpsertRequestViewModel
{
    public string? Name { get; set; }
    public string? NickName { get; set; }
}

