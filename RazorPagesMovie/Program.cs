using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RazorPagesMovie.Data;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Identity;
using RazorPagesMovie.Models;
using RazorPagesMovie.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

// Add DbContext with SQL Server configuration
builder.Services.AddDbContext<RazorPagesMovieContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("RazorPagesMovieContext")
        ?? throw new InvalidOperationException("Connection string 'RazorPagesMovieContext' not found.")));

// Register other services
builder.Services.AddSingleton<InvoiceNoGenerator>();
builder.Services.AddScoped<ReservationService>();
builder.Services.AddScoped<BillingTransactionService>();
builder.Services.AddSingleton<PaginationService>();

// Add the ExchangeRateUpdater background service
builder.Services.AddHostedService<ExchangeRateUpdater>();

// Configure Identity with ApplicationUser and IdentityRole
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;  // Disable email confirmation for demo or dev purposes
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
})
.AddEntityFrameworkStores<RazorPagesMovieContext>()
.AddDefaultTokenProviders();

// Configure cookie authentication for Identity
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Identity/Account/Login";           // Correct login path
    options.AccessDeniedPath = "/Identity/Account/AccessDenied"; // Custom access denied page (optional)
    options.ExpireTimeSpan = TimeSpan.FromMinutes(10);  // Set session timeout
    options.SlidingExpiration = true; // Extend session on activity
    //options.LogoutPath = "Identity/Account/Logout"; // Set your logout path

});

// Enable session management
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(10); // Set idle timeout
    options.Cookie.HttpOnly = true; // Make the session cookie HttpOnly
});

// Set up logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();  // Enforce strict transport security in non-dev environments
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession(); // Enable session management
app.UseAuthentication();  // Enable authentication
app.UseAuthorization();   // Enable authorization

// Redirect to homepage on root access
app.MapGet("/", context =>
{
    context.Response.Redirect("/Homepage");
    return Task.CompletedTask;
});

app.MapRazorPages();  // Map Razor Pages

// Run the application
app.Run();