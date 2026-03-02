// CHANGED_BY_AI: 2026-03-02 - Add category view models
namespace Spendly.Mobile.ViewModels.Finance;

public class CategoryListItemViewModel
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? ParentId { get; set; }
    public string? Color { get; set; }
    public string? Icon { get; set; }
    public int MerchantCount { get; set; }
    public bool IsSystem { get; set; }
}

public class CategoryResponseViewModel : CategoryListItemViewModel
{
}

public class CategoryMerchantItemViewModel
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int TransactionCount { get; set; }
    public DateTime? LastTransactionDate { get; set; }
}

