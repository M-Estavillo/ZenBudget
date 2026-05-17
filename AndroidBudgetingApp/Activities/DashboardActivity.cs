using Android.Content;
using Android.Graphics;
using Android.Views;
using Android.Widget;
using AndroidBudgetingApp.Services;

namespace AndroidBudgetingApp.Activities;

[Activity(Label = "Dashboard", Theme = "@style/AppTheme")]
public class DashboardActivity : Activity
{
    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
        SetContentView(Resource.Layout.activity_dashboard);

        SetupBottomNav("home");

        FindViewById<TextView>(Resource.Id.btnViewAll)!.Click += (_, _) =>
            StartActivity(new Intent(this, typeof(TransactionsActivity)));

        FindViewById(Resource.Id.fabAddExpense)!.Click += (_, _) => ShowAddTransactionDialog();

        LoadDashboardData();
    }

    protected override void OnResume()
    {
        base.OnResume();
        LoadDashboardData();
    }

    private async void LoadDashboardData()
    {
        try
        {
            var profile = await ApiService.Instance.GetProfileAsync();
            if (profile != null) CurrencyHelper.CurrentCurrency = profile.Currency;

            var balance = await ApiService.Instance.GetBalanceAsync();
            FindViewById<TextView>(Resource.Id.textBalance)!.Text = CurrencyHelper.Format(balance);

            var weekly = await ApiService.Instance.GetWeeklySummaryAsync();
            BuildBarChart(weekly);

            var recent = await ApiService.Instance.GetRecentTransactionsAsync(5);
            BuildRecentList(recent);
        }
        catch (Exception ex)
        {
            Toast.MakeText(this, $"Error loading data: {ex.Message}", ToastLength.Short)?.Show();
        }
    }

    private void BuildBarChart(List<WeeklySummaryDto> data)
    {
        var container = FindViewById<LinearLayout>(Resource.Id.chartContainer)!;
        container.RemoveAllViews();

        if (data.Count == 0) return;
        var maxAmount = data.Max(d => d.Amount);
        if (maxAmount == 0) maxAmount = 1;

        foreach (var day in data)
        {
            var heightPct = (float)(day.Amount / maxAmount);
            if (heightPct < 0.05f) heightPct = 0.05f; // min visible height

            var dayLayout = new LinearLayout(this)
            {
                Orientation = Android.Widget.Orientation.Vertical,
                LayoutParameters = new LinearLayout.LayoutParams(0, ViewGroup.LayoutParams.WrapContent, 1f)
            };
            dayLayout.SetGravity(GravityFlags.Bottom | GravityFlags.CenterHorizontal);

            // Add amount label at the top of the bar
            var amountLabel = new TextView(this)
            {
                Text = day.Amount > 0 ? Math.Round(day.Amount).ToString() : "",
                TextSize = 8f,
                LayoutParameters = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent)
            };
            amountLabel.SetTextColor(Color.ParseColor("#71717A")); // zen_muted
            amountLabel.SetTypeface(Typeface.Create("sans-serif", TypefaceStyle.Normal), TypefaceStyle.Bold);
            amountLabel.SetPadding(0, 0, 0, 4);

            var bar = new View(this);
            var barLp = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, (int)(heightPct * 128 * Resources!.DisplayMetrics!.Density));
            barLp.SetMargins(8, 0, 8, 8); // Side margins and bottom margin
            bar.LayoutParameters = barLp;
            bar.SetBackgroundResource(day.IsHighlighted
                ? Resource.Drawable.bg_bar_highlight
                : Resource.Drawable.bg_bar_normal);

            var label = new TextView(this)
            {
                Text = day.DayLabel,
                TextSize = 10f,
                LayoutParameters = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent)
            };
            label.SetTextColor(Color.ParseColor("#A1A1AA")); // zen_muted
            label.SetTypeface(Typeface.Create("sans-serif", TypefaceStyle.Normal), TypefaceStyle.Normal);

            dayLayout.AddView(amountLabel);
            dayLayout.AddView(bar);
            dayLayout.AddView(label);

            container.AddView(dayLayout);
        }
    }

    private void BuildRecentList(List<TransactionDto> transactions)
    {
        var container = FindViewById<LinearLayout>(Resource.Id.recentTransactionsContainer)!;
        container.RemoveAllViews();

        foreach (var tx in transactions)
        {
            var view = LayoutInflater!.Inflate(Resource.Layout.item_transaction, container, false)!;
            view.FindViewById<TextView>(Resource.Id.textDescription)!.Text = tx.Description;
            view.FindViewById<TextView>(Resource.Id.textAmount)!.Text = CurrencyHelper.Format(tx.Amount);

            var timeAgo = GetTimeLabel(tx.Timestamp);
            view.FindViewById<TextView>(Resource.Id.textDate)!.Text = timeAgo;

            view.FindViewById<ImageView>(Resource.Id.imgIcon)!
                .SetImageResource(AndroidBudgetingApp.Services.UIHelper.GetCategoryIcon(tx.CategoryIcon));

            view.Click += (_, _) => ShowTransactionOptionsDialog(tx);

            container.AddView(view);
        }
    }

    private void ShowTransactionOptionsDialog(TransactionDto tx)
    {
        new AlertDialog.Builder(this)
            .SetTitle("Transaction Options")!
            .SetItems(new[] { "Edit", "Delete" }, (s, e) =>
            {
                if (e.Which == 0) ShowEditTransactionDialog(tx);
                else if (e.Which == 1) DeleteTransaction(tx);
            })
            .Show();
    }

    private async void DeleteTransaction(TransactionDto tx)
    {
        new AlertDialog.Builder(this)
            .SetTitle("Confirm Delete")!
            .SetMessage($"Are you sure you want to delete '{tx.Description}'?")!
            .SetPositiveButton("Delete", async (s, e) =>
            {
                try
                {
                    await ApiService.Instance.DeleteTransactionAsync(tx.Id);
                    Toast.MakeText(this, "Transaction deleted", ToastLength.Short)?.Show();
                    LoadDashboardData();
                }
                catch (Exception ex)
                {
                    Toast.MakeText(this, $"Error: {ex.Message}", ToastLength.Short)?.Show();
                }
            })!
            .SetNegativeButton("Cancel", (s, e) => { })!
            .Show();
    }

    private void ShowEditTransactionDialog(TransactionDto tx)
    {
        var dialogView = LayoutInflater!.Inflate(Resource.Layout.dialog_add_transaction, null)!;
        var dialog = new AlertDialog.Builder(this)
            .SetView(dialogView)!
            .Create()!;

        dialogView.FindViewById<TextView>(Resource.Id.textDialogTitle)!.Text = "Edit Expense";
        var editDesc = dialogView.FindViewById<EditText>(Resource.Id.editDescription)!;
        var editAmount = dialogView.FindViewById<EditText>(Resource.Id.editAmount)!;
        var spinner = dialogView.FindViewById<Spinner>(Resource.Id.spinnerCategory)!;
        var btnSelectDate = dialogView.FindViewById<Button>(Resource.Id.btnSelectDate)!;

        editDesc.Text = tx.Description;
        editAmount.Text = CurrencyHelper.FormatPlain(tx.Amount);

        var selectedDate = tx.Timestamp.ToLocalTime();
        btnSelectDate.Text = $"Date: {selectedDate:MMM d, yyyy}";

        btnSelectDate.Click += (_, _) =>
        {
            var datePicker = new Android.App.DatePickerDialog(this, (s, e) =>
            {
                selectedDate = new DateTime(e.Date.Year, e.Date.Month, e.Date.Day, selectedDate.Hour, selectedDate.Minute, selectedDate.Second, DateTimeKind.Local);
                btnSelectDate.Text = $"Date: {selectedDate:MMM d, yyyy}";
            }, selectedDate.Year, selectedDate.Month - 1, selectedDate.Day);
            datePicker.Show();
        };

        // Load categories into spinner
        LoadCategoriesIntoSpinner(spinner);

        // Set selection
        var index = _categoryIds.FindIndex(id => id == tx.CategoryId);
        if (index >= 0) spinner.SetSelection(index);

        dialogView.FindViewById<Button>(Resource.Id.btnCancel)!.Click += (_, _) => dialog.Dismiss();
        dialogView.FindViewById<Button>(Resource.Id.btnSave)!.Click += async (_, _) =>
        {
            var desc = editDesc.Text?.Trim() ?? "";
            var amountText = editAmount.Text?.Trim() ?? "";
            if (string.IsNullOrEmpty(desc) || string.IsNullOrEmpty(amountText))
            {
                Toast.MakeText(this, "Fill in all fields", ToastLength.Short)?.Show();
                return;
            }

            if (!decimal.TryParse(amountText, out var amount))
            {
                Toast.MakeText(this, "Invalid amount", ToastLength.Short)?.Show();
                return;
            }

            try
            {
                var categoryId = _categoryIds.Count > 0 ? _categoryIds[spinner.SelectedItemPosition] : "";
                var amountInPhp = CurrencyHelper.ParseToPhp(amount);
                await ApiService.Instance.UpdateTransactionAsync(tx.Id, desc, -Math.Abs(amountInPhp), categoryId, selectedDate.ToUniversalTime());
                dialog.Dismiss();
                LoadDashboardData();
                Toast.MakeText(this, "Expense updated!", ToastLength.Short)?.Show();
            }
            catch (Exception ex)
            {
                Toast.MakeText(this, $"Error: {ex.Message}", ToastLength.Short)?.Show();
            }
        };

        dialog.Show();
    }

    private void ShowAddTransactionDialog()
    {
        var dialogView = LayoutInflater!.Inflate(Resource.Layout.dialog_add_transaction, null)!;
        var dialog = new AlertDialog.Builder(this)
            .SetView(dialogView)!
            .Create()!;

        var editDesc = dialogView.FindViewById<EditText>(Resource.Id.editDescription)!;
        var editAmount = dialogView.FindViewById<EditText>(Resource.Id.editAmount)!;
        var spinner = dialogView.FindViewById<Spinner>(Resource.Id.spinnerCategory)!;
        var btnSelectDate = dialogView.FindViewById<Button>(Resource.Id.btnSelectDate)!;

        var selectedDate = DateTime.Now;
        btnSelectDate.Text = $"Date: {selectedDate:MMM d, yyyy}";

        btnSelectDate.Click += (_, _) =>
        {
            var datePicker = new Android.App.DatePickerDialog(this, (s, e) =>
            {
                selectedDate = new DateTime(e.Date.Year, e.Date.Month, e.Date.Day, selectedDate.Hour, selectedDate.Minute, selectedDate.Second, DateTimeKind.Local);
                btnSelectDate.Text = $"Date: {selectedDate:MMM d, yyyy}";
            }, selectedDate.Year, selectedDate.Month - 1, selectedDate.Day);
            datePicker.Show();
        };

        // Load categories into spinner
        LoadCategoriesIntoSpinner(spinner);

        dialogView.FindViewById<Button>(Resource.Id.btnCancel)!.Click += (_, _) => dialog.Dismiss();
        dialogView.FindViewById<Button>(Resource.Id.btnSave)!.Click += async (_, _) =>
        {
            var desc = editDesc.Text?.Trim() ?? "";
            var amountText = editAmount.Text?.Trim() ?? "";
            if (string.IsNullOrEmpty(desc) || string.IsNullOrEmpty(amountText))
            {
                Toast.MakeText(this, "Fill in all fields", ToastLength.Short)?.Show();
                return;
            }

            if (!decimal.TryParse(amountText, out var amount))
            {
                Toast.MakeText(this, "Invalid amount", ToastLength.Short)?.Show();
                return;
            }

            try
            {
                var categoryId = _categoryIds.Count > 0 ? _categoryIds[spinner.SelectedItemPosition] : "";
                var amountInPhp = CurrencyHelper.ParseToPhp(amount);
                await ApiService.Instance.CreateTransactionAsync(desc, -Math.Abs(amountInPhp), categoryId, selectedDate.ToUniversalTime());
                dialog.Dismiss();
                LoadDashboardData();
                Toast.MakeText(this, "Expense added!", ToastLength.Short)?.Show();
            }
            catch (Exception ex)
            {
                Toast.MakeText(this, $"Error: {ex.Message}", ToastLength.Short)?.Show();
            }
        };

        dialog.Show();
    }

    private List<string> _categoryIds = new();

    private async void LoadCategoriesIntoSpinner(Spinner spinner)
    {
        try
        {
            var categories = await ApiService.Instance.GetCategoriesAsync();
            _categoryIds = categories.Select(c => c.Id).ToList();
            var names = categories.Select(c => c.Name).ToList();
            var adapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleSpinnerItem, names);
            adapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
            spinner.Adapter = adapter;
        }
        catch { /* Categories not loaded, spinner will be empty */ }
    }

    private static string GetTimeLabel(DateTime timestamp)
    {
        var localTxDate = timestamp.ToLocalTime().Date;
        var localToday = DateTime.Now.Date;
        var diff = localToday - localTxDate;
        
        if (diff.Days == 0) return "Today";
        if (diff.Days == 1) return "Yesterday";
        return localTxDate.ToString("MMM d");
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

/// <summary>
/// Reusable click listener for bottom nav that prevents re-launching the current screen.
/// </summary>
public class NavClickListener : Java.Lang.Object, View.IOnClickListener
{
    private readonly Activity _activity;
    private readonly Type _target;
    private readonly bool _isCurrent;

    public NavClickListener(Activity activity, Type target, bool isCurrent)
    {
        _activity = activity;
        _target = target;
        _isCurrent = isCurrent;
    }

    public void OnClick(View? v)
    {
        if (_isCurrent) return;
        var intent = new Intent(_activity, _target);
        intent.SetFlags(ActivityFlags.ClearTop | ActivityFlags.SingleTop);
        _activity.StartActivity(intent);
#pragma warning disable CA1422
        _activity.OverridePendingTransition(0, 0); // No animation for tab switching
#pragma warning restore CA1422
    }
}
