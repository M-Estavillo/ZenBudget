using Android.Content;
using Android.Graphics;
using Android.Views;
using Android.Widget;
using AndroidBudgetingApp.Services;

namespace AndroidBudgetingApp.Activities;

[Activity(Label = "Transactions", Theme = "@style/AppTheme")]
public class TransactionsActivity : Activity
{
    private string? _selectedCategoryId;
    private List<CategoryDto> _categories = new();

    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
        SetContentView(Resource.Layout.activity_transactions);

        SetupBottomNav("none");

        var editSearch = FindViewById<EditText>(Resource.Id.editSearch)!;
        editSearch.TextChanged += (_, _) => LoadTransactions(editSearch.Text?.Trim());

        LoadCategories();
        LoadTransactions();
    }

    private async void LoadCategories()
    {
        try
        {
            _categories = await ApiService.Instance.GetCategoriesAsync();
            BuildChips();
        }
        catch { /* silent */ }
    }

    private void BuildChips()
    {
        var container = FindViewById<LinearLayout>(Resource.Id.chipContainer)!;
        container.RemoveAllViews();

        // "All" chip
        AddChip(container, "All", null, true);

        foreach (var cat in _categories)
            AddChip(container, cat.Name, cat.Id, false);
    }

    private void AddChip(LinearLayout container, string label, string? categoryId, bool isActive)
    {
        var chip = new TextView(this)
        {
            Text = label,
        };
        chip.SetTextSize(Android.Util.ComplexUnitType.Sp, 12);
        chip.SetPadding(DpToPx(16), DpToPx(6), DpToPx(16), DpToPx(6));

        if (isActive)
        {
            chip.SetBackgroundResource(Resource.Drawable.bg_chip_active);
            chip.SetTextColor(Color.White);
        }
        else
        {
            chip.SetBackgroundResource(Resource.Drawable.bg_chip_inactive);
            chip.SetTextColor(Color.ParseColor("#71717A"));
        }

        var lp = new LinearLayout.LayoutParams(
            LinearLayout.LayoutParams.WrapContent,
            LinearLayout.LayoutParams.WrapContent);
        lp.SetMargins(0, 0, DpToPx(8), 0);
        chip.LayoutParameters = lp;

        chip.Click += (_, _) =>
        {
            _selectedCategoryId = categoryId;
            // Rebuild chips to update active state
            var parent = (LinearLayout)chip.Parent!;
            for (int i = 0; i < parent.ChildCount; i++)
            {
                var child = (TextView)parent.GetChildAt(i)!;
                if (child == chip)
                {
                    child.SetBackgroundResource(Resource.Drawable.bg_chip_active);
                    child.SetTextColor(Color.White);
                }
                else
                {
                    child.SetBackgroundResource(Resource.Drawable.bg_chip_inactive);
                    child.SetTextColor(Color.ParseColor("#71717A"));
                }
            }
            var search = FindViewById<EditText>(Resource.Id.editSearch)?.Text?.Trim();
            LoadTransactions(search);
        };

        container.AddView(chip);
    }

    private async void LoadTransactions(string? search = null)
    {
        try
        {
            var transactions = await ApiService.Instance.GetTransactionsAsync(search, _selectedCategoryId);
            BuildTransactionList(transactions);
        }
        catch (Exception ex)
        {
            Toast.MakeText(this, $"Error: {ex.Message}", ToastLength.Short)?.Show();
        }
    }

    private void BuildTransactionList(List<TransactionDto> transactions)
    {
        var container = FindViewById<LinearLayout>(Resource.Id.transactionListContainer)!;
        container.RemoveAllViews();

        var grouped = transactions.GroupBy(t =>
        {
            var localTxDate = t.Timestamp.ToLocalTime().Date;
            var localToday = DateTime.Now.Date;
            var diff = localToday - localTxDate;
            
            if (diff.Days == 0) return "TODAY";
            if (diff.Days == 1) return "YESTERDAY";
            return localTxDate.ToString("MMM d").ToUpper();
        });

        foreach (var group in grouped)
        {
            // Date header
            var header = new TextView(this)
            {
                Text = group.Key,
            };
            header.SetTextSize(Android.Util.ComplexUnitType.Sp, 10);
            header.SetTextColor(Color.ParseColor("#71717A"));
            header.SetTypeface(Typeface.Default, TypefaceStyle.Bold);
            header.LetterSpacing = 0.2f;
            var hlp = new LinearLayout.LayoutParams(
                LinearLayout.LayoutParams.WrapContent,
                LinearLayout.LayoutParams.WrapContent);
            hlp.SetMargins(0, 0, 0, DpToPx(16));
            header.LayoutParameters = hlp;
            container.AddView(header);

            foreach (var tx in group)
            {
                var view = LayoutInflater!.Inflate(Resource.Layout.item_transaction, container, false)!;
                view.FindViewById<TextView>(Resource.Id.textDescription)!.Text = tx.Description;
                view.FindViewById<TextView>(Resource.Id.textAmount)!.Text = CurrencyHelper.Format(tx.Amount);

                var subtitle = tx.CategoryName ?? "";
                if (tx.Timestamp.Hour > 0) subtitle += $" • {tx.Timestamp.ToLocalTime():h:mm tt}";
                view.FindViewById<TextView>(Resource.Id.textDate)!.Text = subtitle;

                view.FindViewById<ImageView>(Resource.Id.imgIcon)!
                    .SetImageResource(AndroidBudgetingApp.Services.UIHelper.GetCategoryIcon(tx.CategoryIcon));

                view.Click += (_, _) => ShowTransactionOptionsDialog(tx);

                container.AddView(view);
            }

            // Spacer between groups
            var spacer = new View(this);
            spacer.LayoutParameters = new LinearLayout.LayoutParams(
                LinearLayout.LayoutParams.MatchParent, DpToPx(32));
            container.AddView(spacer);
        }
    }

    private void ShowTransactionOptionsDialog(TransactionDto tx)
    {
        new Android.App.AlertDialog.Builder(this)
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
        new Android.App.AlertDialog.Builder(this)
            .SetTitle("Confirm Delete")!
            .SetMessage($"Are you sure you want to delete '{tx.Description}'?")!
            .SetPositiveButton("Delete", async (s, e) =>
            {
                try
                {
                    await ApiService.Instance.DeleteTransactionAsync(tx.Id);
                    Toast.MakeText(this, "Transaction deleted", ToastLength.Short)?.Show();
                    var search = FindViewById<EditText>(Resource.Id.editSearch)?.Text?.Trim();
                    LoadTransactions(search);
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
        var dialog = new Android.App.AlertDialog.Builder(this)
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
        var names = _categories.Select(c => c.Name).ToList();
        var adapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleSpinnerItem, names);
        adapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
        spinner.Adapter = adapter;

        // Set selection
        var index = _categories.FindIndex(c => c.Id == tx.CategoryId);
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
                var categoryId = _categories.Count > 0 ? _categories[spinner.SelectedItemPosition].Id : "";
                var amountInPhp = CurrencyHelper.ParseToPhp(amount);
                await ApiService.Instance.UpdateTransactionAsync(tx.Id, desc, -Math.Abs(amountInPhp), categoryId, selectedDate.ToUniversalTime());
                dialog.Dismiss();
                var search = FindViewById<EditText>(Resource.Id.editSearch)?.Text?.Trim();
                LoadTransactions(search);
                Toast.MakeText(this, "Expense updated!", ToastLength.Short)?.Show();
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
