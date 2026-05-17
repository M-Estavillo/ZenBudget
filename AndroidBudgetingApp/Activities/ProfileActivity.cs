using Android.Content;
using Android.Widget;
using AndroidBudgetingApp.Services;

namespace AndroidBudgetingApp.Activities;

[Activity(Label = "Profile", Theme = "@style/AppTheme")]
public class ProfileActivity : Activity
{
    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
        SetContentView(Resource.Layout.activity_profile);

        SetupBottomNav("profile");

        FindViewById<RelativeLayout>(Resource.Id.btnCurrency)!.Click += (_, _) => ShowCurrencyDialog();

        FindViewById<Button>(Resource.Id.btnSignOut)!.Click += (_, _) =>
        {
            TokenManager.ClearAll(this);
            ApiService.Instance.SetAuthToken(null);
            var intent = new Intent(this, typeof(LoginActivity));
            intent.SetFlags(ActivityFlags.ClearTask | ActivityFlags.NewTask);
            StartActivity(intent);
            Finish();
        };

        LoadProfile();
    }

    private async void LoadProfile()
    {
        try
        {
            var profile = await ApiService.Instance.GetProfileAsync();
            if (profile != null)
            {
                FindViewById<TextView>(Resource.Id.textName)!.Text = profile.FullName;
                FindViewById<TextView>(Resource.Id.textEmail)!.Text = profile.Email;
                var currencyDisplay = profile.Currency switch {
                    "PHP" => "PHP (₱)",
                    "USD" => "USD ($)",
                    "CNY" => "CNY (¥)",
                    "JPY" => "JPY (¥)",
                    "KRW" => "KRW (₩)",
                    _ => profile.Currency
                };
                CurrencyHelper.CurrentCurrency = profile.Currency;
                FindViewById<TextView>(Resource.Id.textCurrency)!.Text = currencyDisplay;
            }
        }
        catch
        {
            // Fall back to stored info
            var (name, email) = TokenManager.GetUserInfo(this);
            if (name != null) FindViewById<TextView>(Resource.Id.textName)!.Text = name;
            if (email != null) FindViewById<TextView>(Resource.Id.textEmail)!.Text = email;
        }
    }

    private void ShowCurrencyDialog()
    {
        var currencies = new[] { "PHP", "USD", "CNY", "JPY", "KRW" };
        var labels = new[] { "PHP (₱)", "USD ($)", "CNY (¥)", "JPY (¥)", "KRW (₩)" };

        new Android.App.AlertDialog.Builder(this)
            .SetTitle("Select Currency")!
            .SetItems(labels, async (s, e) =>
            {
                var selected = currencies[e.Which];
                try
                {
                    var updated = await ApiService.Instance.UpdateProfileAsync(currency: selected);
                    if (updated != null)
                    {
                        CurrencyHelper.CurrentCurrency = selected;
                        FindViewById<TextView>(Resource.Id.textCurrency)!.Text = labels[e.Which];
                        Toast.MakeText(this, "Currency updated", ToastLength.Short)?.Show();
                    }
                }
                catch (Exception ex)
                {
                    Toast.MakeText(this, $"Error: {ex.Message}", ToastLength.Short)?.Show();
                }
            })
            .Show();
    }

    private void SetupBottomNav(string active)
    {
        var navHome = FindViewById<ImageView>(Resource.Id.navHome);
        var navBudget = FindViewById<ImageView>(Resource.Id.navBudget);
        var navAnalytics = FindViewById<ImageView>(Resource.Id.navAnalytics);
        var navProfile = FindViewById<ImageView>(Resource.Id.navProfile);

        navHome?.SetImageResource(active == "home" ? Resource.Drawable.ic_home : Resource.Drawable.ic_home_outline);
        navBudget?.SetImageResource(active == "budget" ? Resource.Drawable.ic_list_active : Resource.Drawable.ic_list);
        navAnalytics?.SetImageResource(active == "analytics" ? Resource.Drawable.ic_chart_active : Resource.Drawable.ic_chart);
        navProfile?.SetImageResource(active == "profile" ? Resource.Drawable.ic_user_active : Resource.Drawable.ic_user);

        navHome?.SetOnClickListener(new NavClickListener(this, typeof(DashboardActivity), active == "home"));
        navBudget?.SetOnClickListener(new NavClickListener(this, typeof(BudgetActivity), active == "budget"));
        navAnalytics?.SetOnClickListener(new NavClickListener(this, typeof(AnalyticsActivity), active == "analytics"));
        navProfile?.SetOnClickListener(new NavClickListener(this, typeof(ProfileActivity), active == "profile"));
    }
}
