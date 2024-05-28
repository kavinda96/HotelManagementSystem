namespace RazorPagesMovie
{
    using Microsoft.AspNetCore.Antiforgery;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.Extensions.DependencyInjection;

    public class AntiForgeryMiddleware
    {
        public void Configure(IApplicationBuilder app)
        {
            // Configure anti-forgery
            app.Use(next => context =>
            {
                var antiforgery = context.RequestServices.GetRequiredService<IAntiforgery>();
                var tokens = antiforgery.GetAndStoreTokens(context);
                context.Response.Cookies.Append("XSRF-TOKEN", tokens.RequestToken, new CookieOptions
                {
                    HttpOnly = false // Make the cookie accessible to JavaScript
                });

                return next(context);
            });
        }
    }

}
