// CHANGED_BY_AI: 2026-03-02 - Add merchant nickname request model
// CHANGED_BY_AI: 2026-03-02 - Add merchant request view models
namespace Spendly.Mobile.ViewModels.Finance;

public class MerchantListRequestViewModel
{
    public string? Search { get; set; }
    public bool? IsUncategorized { get; set; }
    public string? SortBy { get; set; }
    public string? SortDirection { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}

public class MerchantCategoryUpdateRequestViewModel
{
    public string? CategoryId { get; set; }
}

public class MerchantNicknameUpdateRequestViewModel
{
    public string? Nickname { get; set; }
}

public class MerchantUpdateRequestViewModel
{
    public string? CategoryId { get; set; }
    public string? Nickname { get; set; }
}
