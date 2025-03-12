using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Microsoft.AspNetCore.Builder;
using Serilog;

[ExcludeFromCodeCoverage]
public static class WebAppExtensions
{
    public static WebApplication ConfigureApplication(this WebApplication app)
    {

        var IsDevelopment = app.Environment.IsDevelopment();

        // ✅ Use Serilog Request Logging Middleware
        app.UseSerilogRequestLogging();

        // Apply Authentication Middleware
        app.UseAuthentication();
        app.UseAuthorization();

        // -----------------------------------------------------------------------------------------

        if (IsDevelopment)
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        // ✅ Use CORS Middleware before controllers
        app.UseCors(IsDevelopment ? "AllowAll" : "AllowSpecificOrigins"); // Apply the selected CORS policy

        app.UseResponseCaching();

        app.UseHttpsRedirection();
        app.MapControllers();

        // Console.WriteLine(app.Environment.IsDevelopment().ToString());
        app.UseResponseCompression();

        // use rate limiter
        app.UseRateLimiter();

        // Ensure Database is Created
        // using (var scope = app.Services.CreateScope())
        // {
        //     var dbContext = scope.ServiceProvider.GetRequiredService<ExpenseDbContext>();
        //     dbContext.Database.Migrate();
        // }


        // Prevent Cross-Site Scripting (XSS) & Clickjacking
        // Use Content Security Policy (CSP) and X-Frame-Options:

        app.Use(async (context, next) =>
        {
            context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
            context.Response.Headers.Append("X-Frame-Options", "DENY");
            context.Response.Headers.Append("Content-Security-Policy", "default-src 'self'");
            await next();
        });


        app.MapGet("/", () => "Hello, World!");
        app.MapGet("/health", () => "Healthy");

        app.MapGet("/secure", () => "You are authenticated!")
            .RequireAuthorization(); // Protect this endpoint

        app.MapGet("/admin", () => "Welcome Admin!")
            .RequireAuthorization(policy => policy.RequireRole("admin"));



        #region MinimalApi

        // _ = app.MapVersionEndpoints();
        // _ = app.MapAuthorEndpoints();
        // _ = app.MapMovieEndpoints();
        // _ = app.MapReviewEndpoints();

        #endregion MinimalApi

        return app;
    }
}