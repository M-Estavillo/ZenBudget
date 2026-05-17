using Android.Content;
using Android.Graphics;
using Android.Views;
using Android.Widget;
using AndroidBudgetingApp.Services;
using AndroidBudgetingApp.Views;

namespace AndroidBudgetingApp.Activities;

[Activity(Label = "Analytics", Theme = "@style/AppTheme")]
public class AnalyticsActivity : Activity
{
    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
        SetContentView(Resource.Layout.activity_analytics);

        SetupBottomNav("analytics");
        LoadAnalyticsData();
    }

    private async void LoadAnalyticsData()
    {
        try
        {
            // Ring chart data
            var categorySpending = await ApiService.Instance.GetSpendingByCategoryAsync();
            var ringChart = FindViewById<RingChartView>(Resource.Id.ringChart)!;
            ringChart.SetData(categorySpending.Select(c => (c.Percentage * 1f, c.Color)).ToList());

            // Total spending
            var total = await ApiService.Instance.GetTotalSpendingAsync();
            FindViewById<TextView>(Resource.Id.textTotalSpending)!.Text = CurrencyHelper.Format(total);

            // Legend
            BuildLegend(categorySpending);

            // Monthly trend
            var monthlyTrend = await ApiService.Instance.GetMonthlyTrendAsync();
            BuildMonthlyChart(monthlyTrend);
        }
        catch (Exception ex)
        {
            Toast.MakeText(this, $"Error: {ex.Message}", ToastLength.Short)?.Show();
        }
    }

    private void BuildLegend(List<CategorySpendingDto> data)
    {
        var container = FindViewById<LinearLayout>(Resource.Id.legendContainer)!;
        container.RemoveAllViews();

        foreach (var item in data)
        {
            var row = new LinearLayout(this)
            {
                Orientation = Orientation.Horizontal,
            };
            row.SetGravity(GravityFlags.CenterVertical);
            var rlp = new LinearLayout.LayoutParams(
                LinearLayout.LayoutParams.MatchParent,
                LinearLayout.LayoutParams.WrapContent);
            rlp.SetMargins(0, 0, 0, DpToPx(12));
            row.LayoutParameters = rlp;

            // Left side: dot + name
            var leftSide = new LinearLayout(this) { Orientation = Orientation.Horizontal };
            leftSide.SetGravity(GravityFlags.CenterVertical);
            leftSide.LayoutParameters = new LinearLayout.LayoutParams(0, LinearLayout.LayoutParams.WrapContent, 1f);

            var dot = new View(this);
            dot.LayoutParameters = new LinearLayout.LayoutParams(DpToPx(8), DpToPx(8));
            ((LinearLayout.LayoutParams)dot.LayoutParameters).SetMargins(0, 0, DpToPx(8), 0);
            var dotDrawable = new Android.Graphics.Drawables.GradientDrawable();
            dotDrawable.SetShape(Android.Graphics.Drawables.ShapeType.Oval);
            dotDrawable.SetColor(Color.ParseColor(item.Color));
            dot.Background = dotDrawable;
            leftSide.AddView(dot);

            var nameText = new TextView(this) { Text = item.Name };
            nameText.SetTextSize(Android.Util.ComplexUnitType.Sp, 12);
            nameText.SetTextColor(Color.ParseColor("#1A1A1A"));
            leftSide.AddView(nameText);

            row.AddView(leftSide);

            // Right side: percentage
            var pctText = new TextView(this) { Text = $"{item.Percentage}%" };
            pctText.SetTextSize(Android.Util.ComplexUnitType.Sp, 12);
            pctText.SetTextColor(Color.ParseColor("#1A1A1A"));
            row.AddView(pctText);

            container.AddView(row);
        }
    }

    private void BuildMonthlyChart(List<MonthlyTrendDto> data)
    {
        var container = FindViewById<LinearLayout>(Resource.Id.monthlyChartContainer)!;
        container.RemoveAllViews();

        if (data.Count == 0) return;
        var maxAmount = data.Max(d => d.Amount);
        if (maxAmount == 0) maxAmount = 1;

        foreach (var month in data)
        {
            var heightPct = (float)(month.Amount / maxAmount);
            if (heightPct < 0.1f) heightPct = 0.1f;

            var monthLayout = new LinearLayout(this)
            {
                Orientation = Android.Widget.Orientation.Vertical,
                LayoutParameters = new LinearLayout.LayoutParams(DpToPx(40), ViewGroup.LayoutParams.WrapContent)
            };
            monthLayout.SetGravity(GravityFlags.Bottom | GravityFlags.CenterHorizontal);

            var bar = new View(this);
            var barLp = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, (int)(heightPct * 128 * Resources!.DisplayMetrics!.Density));
            barLp.SetMargins(DpToPx(2), 0, DpToPx(2), DpToPx(4)); // reduced margins since there are 12 items
            bar.LayoutParameters = barLp;
            bar.SetBackgroundResource(month.IsHighlighted
                ? Resource.Drawable.bg_bar_highlight
                : Resource.Drawable.bg_bar_normal);

            var label = new TextView(this)
            {
                Text = month.Month,
                TextSize = 8f, // smaller text to fit 12 labels
                LayoutParameters = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent)
            };
            label.SetTextColor(Color.ParseColor("#A1A1AA")); // zen_muted
            label.SetTypeface(Typeface.Create("sans-serif", TypefaceStyle.Normal), TypefaceStyle.Normal);

            monthLayout.AddView(bar);
            monthLayout.AddView(label);

            container.AddView(monthLayout);
        }
    }

    private int DpToPx(int dp) => (int)(dp * (Resources?.DisplayMetrics?.Density ?? 3f));

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
