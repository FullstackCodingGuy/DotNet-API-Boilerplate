using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Serilog;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using Microsoft.EntityFrameworkCore;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using NetAPI.Infrastructure;

[ExcludeFromCodeCoverage]
public static class WebAppBuilderExtension
{
    public static WebApplicationBuilder ConfigureApplicationBuilder(this WebApplicationBuilder builder)
    {


        var IsDevelopment = builder.Environment.IsDevelopment();

        #region ✅ Configure Serilog
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()   // Log to console
            .WriteTo.File("logs/api-log.txt", rollingInterval: RollingInterval.Day) // Log to a file (daily rotation)
                                                                                    //.WriteTo.Seq("http://localhost:5341")  // Optional: Centralized logging with Seq
            .Enrich.FromLogContext()
            .MinimumLevel.Information()
            .CreateLogger();

        // ✅ Replace default logging with Serilog
        builder.Host.UseSerilog();

        // -----------------------------------------------------------------------------------------
        #endregion ✅ Configure Serilog


        // ✅ Use proper configuration access for connection string
        // var connectionString = configuration["ConnectionStrings:DefaultConnection"] ?? "Data Source=expensemanager.db";

        // builder.Services.AddDbContext<ExpenseDbContext>(options =>
        //     options.UseSqlite(connectionString));

        builder.Services.AddResponseCaching(); // Enable Response caching

        builder.Services.AddMemoryCache(); // Enable in-memory caching


        #region ✅ Authentication & Authorization

        // Add Authentication
        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.Authority = "http://localhost:8080/realms/master";
                options.Audience = "my-dotnet-api"; // Must match the Keycloak Client ID
                options.RequireHttpsMetadata = false; // Disable in development
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidAudiences = new string[] { "master-realm", "account", "my-dotnet-api" },
                    ValidateIssuerSigningKey = true
                };
            });

        builder.Services.AddAuthorization();
        // -----------------------------------------------------------------------------------------


        #endregion

        #region ✅ Serialisation

        // Use System.Text.Json instead of Newtonsoft.Json for better performance.

        // Enable reference handling and lower casing for smaller responses:
        // Explanation
        // JsonNamingPolicy.CamelCase → Converts property names to camelCase (recommended for APIs).
        // ReferenceHandler.Preserve → Prevents circular reference issues when serializing related entities.


        _ = builder.Services.Configure<JsonOptions>(opt =>
        {
            opt.SerializerOptions.ReferenceHandler = ReferenceHandler.Preserve;
            opt.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
            opt.SerializerOptions.PropertyNameCaseInsensitive = true;
            opt.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            opt.SerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
        });

        #endregion Serialisation

        #region ✅ CORS Policy


        // ✅ Add CORS policy
        // WithOrigins("https://yourfrontend.com") → Restricts access to a specific frontend.
        // AllowAnyMethod() → Allows all HTTP methods (GET, POST, PUT, DELETE, etc.).
        // AllowAnyHeader() → Allows any request headers.
        // AllowCredentials() → Enables sending credentials like cookies, tokens (⚠️ Only works with a specific origin, not *).
        // AllowAnyOrigin() → Enables unrestricted access (only for local testing).

        builder.Services.AddCors(options =>
        {
            // To allow prod specific origins
            options.AddPolicy("AllowSpecificOrigins", policy =>
            {
                policy.WithOrigins("https://yourfrontend.com") // Replace with your frontend URL
                      .AllowAnyMethod()
                      .AllowAnyHeader()
                      .AllowCredentials(); // If authentication cookies/tokens are needed
            });

            // To allow unrestricted access (only for non prod testing)
            options.AddPolicy("AllowAll", policy =>
            {
                policy.AllowAnyOrigin() // Use only for testing; NOT recommended for production
                      .AllowAnyMethod()
                      .AllowAnyHeader();
            });
        });

        // -----------------------------------------------------------------------------------------


        #endregion

        #region ✅ Rate Limiting & Request Payload Threshold


        // Use Rate Limiting
        // Prevent API abuse by implementing rate limiting
        // Add Rate Limiting Middleware
        // Now, each client IP gets 10 requests per minute, with a queue of 2 extra requests.

        builder.Services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests; // Return 429 when limit is exceeded

            // ✅ Explicitly specify type <string> for AddPolicy
            options.AddPolicy<string>("fixed", httpContext =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "default",
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 10, // Allow 10 requests
                        Window = TimeSpan.FromMinutes(1), // Per 1-minute window
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = 2 // Allow 2 extra requests in queue
                    }));
        });

        // Limit Request Size
        // Prevent DoS attacks by limiting payload size.
        // https://learn.microsoft.com/en-us/aspnet/core/fundamentals/servers/kestrel/options?view=aspnetcore-9.0
        builder.WebHost.ConfigureKestrel(serverOptions =>
        {
            serverOptions.Limits.MaxRequestBodySize = 100_000_000;
        });

        // -----------------------------------------------------------------------------------------


        #endregion

        builder.Services.AddInfra();

        builder.Services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.Preserve;
            });

        if (IsDevelopment)
        {
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
        }

        // Enable Compression to reduce payload size
        builder.Services.AddResponseCompression(options =>
        {
            options.EnableForHttps = true;
        });

        return builder;
    }
}