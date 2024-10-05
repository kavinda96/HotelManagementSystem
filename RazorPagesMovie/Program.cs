using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RazorPagesMovie.Data;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity.UI.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddDbContext<RazorPagesMovieContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("RazorPagesMovieContext") ?? throw new InvalidOperationException("Connection string 'RazorPagesMovieContext' not found.")));
builder.Services.AddSingleton<RazorPagesMovie.Services.InvoiceNoGenerator>();
builder.Services.AddScoped<RazorPagesMovie.Services.ReservationService>();
builder.Services.AddScoped<RazorPagesMovie.Services.BillingTransactionService>();
builder.Services.AddSingleton<RazorPagesMovie.Services.PaginationService>();

// Configure Identity with roles
builder.Services.AddDefaultIdentity<IdentityUser>(options =>
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
    // This ensures that the application loads the login page by default
    endpoints.MapGet("/", context =>
    {
        context.Response.Redirect("/Homepage");
        return Task.CompletedTask;
    });
    endpoints.MapRazorPages();
});

// Seed the admin user
//await SeedAdminUser(app.Services);

//async Task SeedAdminUser(IServiceProvider serviceProvider)
//{
//    using (var scope = serviceProvider.CreateScope()) // Create a scope
//    {
//        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
//        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

//        // Create admin role if it doesn't exist
//        if (await roleManager.FindByNameAsync("Admin") == null)
//        {
//            await roleManager.CreateAsync(new IdentityRole("Admin"));
//        }

//        // Create admin user if it doesn't exist
//        var adminUser = await userManager.FindByNameAsync("admin@example.com");
//        if (adminUser == null)
//        {
//            adminUser = new IdentityUser
//            {
//                UserName = "admin@example.com",
//                Email = "admin@example.com"
//            };
//            var createResult = await userManager.CreateAsync(adminUser, "Admin@123"); // Set password
//            if (createResult.Succeeded)
//            {
//                await userManager.AddToRoleAsync(adminUser, "Admin"); // Assign admin role
//            }
//            else
//            {
//                // Log errors if user creation failed
//                foreach (var error in createResult.Errors)
//                {
//                    Console.WriteLine(error.Description);
//                }
//            }
//        }
//    }
//}
app.Run();