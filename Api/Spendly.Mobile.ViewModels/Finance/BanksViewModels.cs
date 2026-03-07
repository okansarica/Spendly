// CHANGED_BY_AI: 2026-03-06 - Add bank and account view models
namespace Spendly.Mobile.ViewModels.Finance;

public class BankListItemViewModel
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? BankDefinitionId { get; set; }
    public bool IsConnected { get; set; }
    public List<BankAccountListItemViewModel> Accounts { get; set; } = [];
}

public class BankAccountListItemViewModel
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsConnected { get; set; }
    public string? CardLast4Digits { get; set; }
}

public class BankDefinitionListItemViewModel
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? LogoName { get; set; }
}
