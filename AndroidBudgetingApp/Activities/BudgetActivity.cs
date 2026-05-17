using Android.Content;
using Android.Graphics;
using Android.Views;
using Android.Widget;
using AndroidBudgetingApp.Services;

namespace AndroidBudgetingApp.Activities;

[Activity(Label = "Budget", Theme = "@style/AppTheme")]
public class BudgetActivity : Activity
{
    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
        SetContentView(Resource.Layout.activity_budget);

        SetupBottomNav("budget");

        FindViewById(Resource.Id.btnAddCategory)!.Click += (_, _) => ShowAddCategoryDialog();

        LoadBudgetData();
    }

    protected override void OnResume()
    {
        base.OnResume();
        LoadBudgetData();
    }

    private async void LoadBudgetData()
    {
        try
        {
            var categories = await ApiService.Instance.GetCategoriesAsync();
            BuildBudgetCards(categories);
        }
        catch (Exception ex)
        {
            Toast.MakeText(this, $"Error: {ex.Message}", ToastLength.Short)?.Show();
        }
    }

    private void BuildBudgetCards(List<CategoryDto> categories)
    {
        var container = FindViewById<LinearLayout>(Resource.Id.budgetCardsContainer)!;
        container.RemoveAllViews();

        foreach (var cat in categories)
        {
            var view = LayoutInflater!.Inflate(Resource.Layout.item_budget_category, container, false)!;

            view.FindViewById<TextView>(Resource.Id.textCategoryName)!.Text = cat.Name;
            view.FindViewById<TextView>(Resource.Id.textCategorySpending)!.Text =
                $"{CurrencyHelper.Format(cat.CurrentSpending)} of {CurrencyHelper.Format(cat.BudgetLimit)} used";

            var pctText = view.FindViewById<TextView>(Resource.Id.textPercentage)!;
            pctText.Text = $"{cat.Percentage}%";

            // Over-budget styling (cobalt text)
            if (cat.Percentage > 100)
            {
                pctText.SetTextColor(Color.ParseColor("#2E5BFF"));
                view.FindViewById<TextView>(Resource.Id.textCategorySpending)!
                    .SetTextColor(Color.ParseColor("#2E5BFF"));
            }

            // Progress bar width
            var progressBar = view.FindViewById<View>(Resource.Id.progressBar)!;
            progressBar.Post(() =>
            {
                var parent = (FrameLayout)progressBar.Parent!;
                var maxWidth = parent.Width;
                var pct = Math.Min(cat.Percentage, 100) / 100f;
                progressBar.LayoutParameters!.Width = (int)(maxWidth * pct);
                progressBar.RequestLayout();
            });

            progressBar.SetBackgroundColor(Color.ParseColor(cat.Color));

            // Set outline color to match the pie chart
            int pl = view.PaddingLeft;
            int pt = view.PaddingTop;
            int pr = view.PaddingRight;
            int pb = view.PaddingBottom;

            var bg = new Android.Graphics.Drawables.GradientDrawable();
            bg.SetShape(Android.Graphics.Drawables.ShapeType.Rectangle);
            bg.SetCornerRadius(DpToPx(12));
            bg.SetColor(Color.Transparent);
            bg.SetStroke(DpToPx(1), Color.ParseColor(cat.Color));
            view.Background = bg;
            view.SetPadding(pl, pt, pr, pb);

            view.Click += (_, _) => ShowEditCategoryDialog(cat);

            container.AddView(view);
        }
    }

    private void ShowAddCategoryDialog()
    {
        var dialogView = LayoutInflater!.Inflate(Resource.Layout.dialog_add_category, null)!;
        var dialog = new AlertDialog.Builder(this).SetView(dialogView)!.Create()!;

        var editName = dialogView.FindViewById<EditText>(Resource.Id.editCategoryName)!;
        var editLimit = dialogView.FindViewById<EditText>(Resource.Id.editBudgetLimit)!;

        dialogView.FindViewById<Button>(Resource.Id.btnCancel)!.Click += (_, _) => dialog.Dismiss();
        dialogView.FindViewById<Button>(Resource.Id.btnSave)!.Click += async (_, _) =>
        {
            var name = editName.Text?.Trim() ?? "";
            if (string.IsNullOrEmpty(name))
            {
                Toast.MakeText(this, "Enter a category name", ToastLength.Short)?.Show();
                return;
            }

            decimal.TryParse(editLimit.Text?.Trim(), out var limit);

            try
            {
                var amountInPhp = CurrencyHelper.ParseToPhp(limit);
                await ApiService.Instance.CreateCategoryAsync(name, amountInPhp);
                dialog.Dismiss();
                LoadBudgetData();
                Toast.MakeText(this, "Category added!", ToastLength.Short)?.Show();
            }
            catch (Exception ex)
            {
                Toast.MakeText(this, $"Error: {ex.Message}", ToastLength.Short)?.Show();
            }
        };

        dialog.Show();
    }

    private void ShowEditCategoryDialog(CategoryDto cat)
    {
        var dialogView = LayoutInflater!.Inflate(Resource.Layout.dialog_add_category, null)!;
        var dialog = new AlertDialog.Builder(this).SetView(dialogView)!.Create()!;

        var title = dialogView.FindViewById<TextView>(Resource.Id.textDialogTitle);
        if (title != null) title.Text = "Edit Category";

        var editName = dialogView.FindViewById<EditText>(Resource.Id.editCategoryName)!;
        var editLimit = dialogView.FindViewById<EditText>(Resource.Id.editBudgetLimit)!;

        editName.Text = cat.Name;
        editLimit.Text = CurrencyHelper.FormatPlain(cat.BudgetLimit);

        dialogView.FindViewById<Button>(Resource.Id.btnCancel)!.Click += (_, _) => dialog.Dismiss();
        dialogView.FindViewById<Button>(Resource.Id.btnSave)!.Click += async (_, _) =>
        {
            var name = editName.Text?.Trim() ?? "";
            if (string.IsNullOrEmpty(name))
            {
                Toast.MakeText(this, "Enter a category name", ToastLength.Short)?.Show();
                return;
            }

            decimal.TryParse(editLimit.Text?.Trim(), out var limit);

            try
            {
                var amountInPhp = CurrencyHelper.ParseToPhp(limit);
                await ApiService.Instance.UpdateCategoryAsync(cat.Id, name, amountInPhp);
                dialog.Dismiss();
                LoadBudgetData();
                Toast.MakeText(this, "Category updated!", ToastLength.Short)?.Show();
            }
            catch (Exception ex)
            {
                Toast.MakeText(this, $"Error: {ex.Message}", ToastLength.Short)?.Show();
            }
        };

        dialog.Show();
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
