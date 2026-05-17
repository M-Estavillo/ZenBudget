namespace AndroidBudgetingApp.Services;

public static class UIHelper
{
    public static int GetCategoryIcon(string? iconName)
    {
        if (string.IsNullOrEmpty(iconName)) return Resource.Drawable.ic_default_category;

        return iconName switch
        {
            "ph:fork-knife-thin" => Resource.Drawable.ic_food,
            "ph:car-thin" => Resource.Drawable.ic_transport,
            "ph:music-notes-thin" => Resource.Drawable.ic_entertainment,
            "ph:house-thin" => Resource.Drawable.ic_housing,
            "ph:heartbeat-thin" => Resource.Drawable.ic_health,
            "ph:shopping-cart-thin" => Resource.Drawable.ic_groceries,
            _ => Resource.Drawable.ic_default_category
        };
    }
}
