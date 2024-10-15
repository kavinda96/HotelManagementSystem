using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RazorPagesMovie.Data;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.Cookies;
using RazorPagesMovie.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddDbContext<RazorPagesMovieContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("RazorPagesMovieContext") ?? throw new InvalidOperationException("Connection string 'RazorPagesMovieContext' not found.")));

// Add other services
builder.Services.AddSingleton<RazorPagesMovie.Services.InvoiceNoGenerator>();
builder.Services.AddScoped<RazorPagesMovie.Services.ReservationService>();
builder.Services.AddScoped<RazorPagesMovie.Services.BillingTransactionService>();
builder.Services.AddSingleton<RazorPagesMovie.Services.PaginationService>();

// Configure Identity with ApplicationUser
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = false; // Disable email confirmation
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
})
.AddEntityFrameworkStores<RazorPagesMovieContext>()
.AddDefaultTokenProviders();

// Configure authentication
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Identity/Account/Login"; // Ensure the login path is correct
    options.AccessDeniedPath = "/Identity/Account/AccessDenied"; // Optional: custom access denied page
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
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication(); // Enable authentication
app.UseAuthorization(); // Enable authorization

app.MapRazorPages();

app.UseEndpoints(endpoints =>
{
    // Redirect to homepage on default access
    endpoints.MapGet("/", context =>
    {
        context.Response.Redirect("/Homepage");
        return Task.CompletedTask;
    });
    endpoints.MapRazorPages();
});



app.Run();
