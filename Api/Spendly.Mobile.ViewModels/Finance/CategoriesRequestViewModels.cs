// CHANGED_BY_AI: 2026-03-02 - Add category request view models
namespace Spendly.Mobile.ViewModels.Finance;

public class CategoryListRequestViewModel
{
}

public class CategoryUpsertRequestViewModel
{
    public string? Name { get; set; }
    public string? ParentId { get; set; }
    public string? Color { get; set; }
    public string? Icon { get; set; }
    public List<string> MerchantIds { get; set; } = [];
}

public class CategoryMerchantsRequestViewModel
{
    public List<string>? MerchantIds { get; set; }
}

