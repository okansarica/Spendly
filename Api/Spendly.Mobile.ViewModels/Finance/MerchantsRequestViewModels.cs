// CHANGED_BY_AI: 2026-03-02 - Add merchant nickname request model
// CHANGED_BY_AI: 2026-03-02 - Add merchant request view models
namespace Spendly.Mobile.ViewModels.Finance;

public class MerchantListRequestViewModel
{
    public bool? IsUncategorized { get; set; }
}

public class MerchantCategoryUpdateRequestViewModel
{
    public string? CategoryId { get; set; }
}


public class MerchantUpdateRequestViewModel
{
    public string? CategoryId { get; set; }
    public string? Nickname { get; set; }
}

public class MerchantCreateRequestViewModel
{
    public string Name { get; set; } = string.Empty;
    public string? Nickname { get; set; }
    public string? CategoryId { get; set; }
}
