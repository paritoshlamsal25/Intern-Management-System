using System;
using System.Threading.Tasks;
using IMS.Web.Data;
using IMS.Web.Middleware;
using IMS.Web.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = WebApplication.CreateBuilder(args);

// ==========================================
// 1. AUTHENTICATION & COOKIE SECURITY
// ==========================================
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;

        // Security Hardening
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    });

// ==========================================
// 2. AUTHORIZATION POLICIES
// ==========================================
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("InternOnly", policy => policy.RequireRole("Intern"));
    options.AddPolicy("HrOnly", policy => policy.RequireRole("HR"));
    options.AddPolicy("AdminOrHr", policy => policy.RequireRole("Admin", "HR"));
});

// ==========================================
// 3. MVC SETUP WITH GLOBAL AUTHORIZE FILTER
// ==========================================
builder.Services.AddControllersWithViews(options =>
{
    // Enforces authentication globally across all Controllers & APIs by default
    options.Filters.Add(new AuthorizeFilter());
});

// ==========================================
// 4. DATABASE CONTEXT
// ==========================================

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
           .ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning)));

// ==========================================
// 5. APPLICATION SERVICES & DEPENDENCY INJECTION
// ==========================================
builder.Services.AddSingleton<IPasswordHasherService, PasswordHasherService>();
builder.Services.AddScoped<IAuditService, AuditService>();
builder.Services.AddScoped<IProgressReportService, ProgressReportService>();

var app = builder.Build();

// ==========================================
// 6. DATABASE MIGRATION & SEEDING (ASYNC)
// ==========================================
await SeedDatabaseAsync(app);

// ==========================================
// 7. MIDDLEWARE PIPELINE ORDERING
// ==========================================
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// Global exception handling middleware
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseHttpsRedirection();

// Serve static files before routing to bypass auth overhead for CSS/JS
app.MapStaticAssets();

app.UseRouting();

// Authentication MUST come before Authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();

// Helper method for clean and safe database initialization
static async Task SeedDatabaseAsync(IHost app)
{
    using var scope = app.Services.CreateScope();
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();

    try
    {
        logger.LogInformation("Initializing database and applying migrations...");
        await DbInitializer.SeedAsync(services);
        logger.LogInformation("Database migration and seeding completed successfully.");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while initializing or seeding the database.");
    }
}