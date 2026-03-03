// CHANGED_BY_AI: 2026-03-02 - Add merchant nickname to view models
// CHANGED_BY_AI: 2026-03-02 - Remove transaction fields from merchant view models
namespace Spendly.Mobile.ViewModels.Finance;

public class MerchantListItemViewModel
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Nickname { get; set; }
    public string? CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
}

public class MerchantDetailViewModel
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Nickname { get; set; }
    public string? CategoryId { get; set; }
}
