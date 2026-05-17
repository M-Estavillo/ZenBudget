using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ZenBudget.Api.Data;
using ZenBudget.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// ── Database (NeonDB / PostgreSQL) ──
builder.Services.AddDbContext<ZenBudgetDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// ── JWT Authentication ──
var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("JWT key is not configured.");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

builder.Services.AddAuthorization();

// ── Services (Dependency Injection) ──
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<TransactionService>();
builder.Services.AddScoped<CategoryService>();
builder.Services.AddScoped<AnalyticsService>();

// ── Controllers ──
builder.Services.AddControllers();

// ── CORS (allow Android emulator/device to connect) ──
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// ── Apply pending migrations on startup ──
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ZenBudgetDbContext>();
    await db.Database.MigrateAsync();
}

// ── Middleware Pipeline ──
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// ── Start ──
Console.WriteLine("🧘 ZenBudget API running on http://localhost:5000");
app.Run();
