using Android.Content;
using Android.Widget;
using AndroidBudgetingApp.Services;

namespace AndroidBudgetingApp.Activities;

[Activity(Label = "ZenBudget", MainLauncher = true, Theme = "@style/AppTheme")]
public class LoginActivity : Activity
{
    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);

        // If already logged in, go straight to dashboard
        if (TokenManager.IsLoggedIn(this))
        {
            var token = TokenManager.GetToken(this);
            ApiService.Instance.SetAuthToken(token);
            StartActivity(new Intent(this, typeof(DashboardActivity)));
            Finish();
            return;
        }

        SetContentView(Resource.Layout.activity_login);

        var editEmail = FindViewById<EditText>(Resource.Id.editEmail)!;
        var editPassword = FindViewById<EditText>(Resource.Id.editPassword)!;
        var btnSignIn = FindViewById<Button>(Resource.Id.btnSignIn)!;
        var btnGoToSignUp = FindViewById<TextView>(Resource.Id.btnGoToSignUp)!;

        btnSignIn.Click += async (_, _) =>
        {
            var email = editEmail.Text?.Trim() ?? "";
            var password = editPassword.Text ?? "";

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                Toast.MakeText(this, "Please fill in all fields", ToastLength.Short)?.Show();
                return;
            }

            btnSignIn.Enabled = false;
            btnSignIn.Text = "Signing in...";

            try
            {
                var result = await ApiService.Instance.LoginAsync(email, password);
                if (result != null)
                {
                    TokenManager.SaveToken(this, result.Token);
                    TokenManager.SaveUserInfo(this, result.User.FullName, result.User.Email);
                    ApiService.Instance.SetAuthToken(result.Token);

                    StartActivity(new Intent(this, typeof(DashboardActivity)));
                    Finish();
                }
            }
            catch (Exception ex)
            {
                Toast.MakeText(this, $"Login failed: {ex.Message}", ToastLength.Long)?.Show();
            }
            finally
            {
                btnSignIn.Enabled = true;
                btnSignIn.Text = "Sign In";
            }
        };

        btnGoToSignUp.Click += (_, _) =>
        {
            StartActivity(new Intent(this, typeof(SignUpActivity)));
        };
    }
}
