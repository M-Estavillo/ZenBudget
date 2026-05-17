using Android.Content;

namespace AndroidBudgetingApp.Services;

/// <summary>
/// Stores and retrieves the JWT token from Android SharedPreferences.
/// </summary>
public static class TokenManager
{
    private const string PrefsName = "ZenBudgetPrefs";
    private const string TokenKey = "jwt_token";
    private const string UserNameKey = "user_name";
    private const string UserEmailKey = "user_email";

    public static void SaveToken(Context context, string token)
    {
        var prefs = context.GetSharedPreferences(PrefsName, FileCreationMode.Private);
        prefs?.Edit()?.PutString(TokenKey, token)?.Apply();
    }

    public static string? GetToken(Context context)
    {
        var prefs = context.GetSharedPreferences(PrefsName, FileCreationMode.Private);
        return prefs?.GetString(TokenKey, null);
    }

    public static void SaveUserInfo(Context context, string name, string email)
    {
        var prefs = context.GetSharedPreferences(PrefsName, FileCreationMode.Private);
        prefs?.Edit()
            ?.PutString(UserNameKey, name)
            ?.PutString(UserEmailKey, email)
            ?.Apply();
    }

    public static (string? Name, string? Email) GetUserInfo(Context context)
    {
        var prefs = context.GetSharedPreferences(PrefsName, FileCreationMode.Private);
        return (prefs?.GetString(UserNameKey, null), prefs?.GetString(UserEmailKey, null));
    }

    public static void ClearAll(Context context)
    {
        var prefs = context.GetSharedPreferences(PrefsName, FileCreationMode.Private);
        prefs?.Edit()?.Clear()?.Apply();
    }

    public static bool IsLoggedIn(Context context) => !string.IsNullOrEmpty(GetToken(context));
}
