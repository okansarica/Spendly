// CHANGED_BY_AI: 2026-03-02 - Add merchant nickname request model
// CHANGED_BY_AI: 2026-03-02 - Add merchant request view models
namespace Spendly.Mobile.ViewModels.Finance;

public class MerchantListRequestViewModel
{
}

public class UserMerchantCategoryUpdateRequestViewModel
{
    public string? CategoryId { get; set; }
}


public class UserMerchantUpdateRequestViewModel
{
    public string? CategoryId { get; set; }
    public string? Nickname { get; set; }
}
