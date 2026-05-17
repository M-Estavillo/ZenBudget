using Android.Content;
using Android.Widget;
using AndroidBudgetingApp.Services;

namespace AndroidBudgetingApp.Activities;

[Activity(Label = "Sign Up", Theme = "@style/AppTheme")]
public class SignUpActivity : Activity
{
    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
        SetContentView(Resource.Layout.activity_signup);

        var editFullName = FindViewById<EditText>(Resource.Id.editFullName)!;
        var editEmail = FindViewById<EditText>(Resource.Id.editEmail)!;
        var editPassword = FindViewById<EditText>(Resource.Id.editPassword)!;
        var btnCreateAccount = FindViewById<Button>(Resource.Id.btnCreateAccount)!;
        var btnGoToLogin = FindViewById<TextView>(Resource.Id.btnGoToLogin)!;
        var btnBack = FindViewById(Resource.Id.btnBack)!;

        btnCreateAccount.Click += async (_, _) =>
        {
            var name = editFullName.Text?.Trim() ?? "";
            var email = editEmail.Text?.Trim() ?? "";
            var password = editPassword.Text ?? "";

            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                Toast.MakeText(this, "Please fill in all fields", ToastLength.Short)?.Show();
                return;
            }

            if (password.Length < 8)
            {
                Toast.MakeText(this, "Password must be at least 8 characters", ToastLength.Short)?.Show();
                return;
            }

            btnCreateAccount.Enabled = false;
            btnCreateAccount.Text = "Creating account...";

            try
            {
                var result = await ApiService.Instance.RegisterAsync(name, email, password);
                if (result != null)
                {
                    Toast.MakeText(this, "Account created! Please sign in.", ToastLength.Short)?.Show();
                    Finish(); // Go back to login
                }
            }
            catch (Exception ex)
            {
                Toast.MakeText(this, $"Registration failed: {ex.Message}", ToastLength.Long)?.Show();
            }
            finally
            {
                btnCreateAccount.Enabled = true;
                btnCreateAccount.Text = "Create Account";
            }
        };

        btnGoToLogin.Click += (_, _) => Finish();
        btnBack.Click += (_, _) => Finish();
    }
}
