### **Types of Middleware in .NET 9 & Examples**  

Middleware in **.NET 9** is software that processes requests and responses in an ASP.NET Core application. It operates in a pipeline, where each middleware component can process the request before passing it to the next middleware and modify the response on the way back.  

---

## **📌 Types of Middleware in .NET 9**  

### **1️⃣ Built-in Middleware**  
.NET provides several built-in middleware components for common functionalities:  

✅ **Routing Middleware** → Controls request routing  
✅ **Authentication & Authorization Middleware** → Manages user authentication and access control  
✅ **Exception Handling Middleware** → Handles errors and exceptions  
✅ **Logging Middleware** → Logs request details  
✅ **Response Compression Middleware** → Compresses responses to improve performance  
✅ **CORS Middleware** → Allows cross-origin requests  
✅ **Static Files Middleware** → Serves static files like CSS, JS, and images  

### **2️⃣ Custom Middleware**  
You can create **custom middleware** to implement logging, request modifications, response modifications, etc.  

---

## **🛠 Examples of Middleware in .NET 9**

### **✅ 1. Exception Handling Middleware**
Handles exceptions globally and returns a proper response.  
```csharp
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred processing the request");
            context.Response.StatusCode = 500;
            await context.Response.WriteAsync("An unexpected error occurred.");
        }
    }
}

// Register in Program.cs
app.UseMiddleware<ExceptionHandlingMiddleware>();
```

---

### **✅ 2. Logging Middleware**
Logs incoming requests and outgoing responses.  
```csharp
public class LoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<LoggingMiddleware> _logger;

    public LoggingMiddleware(RequestDelegate next, ILogger<LoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        _logger.LogInformation($"Request: {context.Request.Method} {context.Request.Path}");
        await _next(context);
        _logger.LogInformation($"Response: {context.Response.StatusCode}");
    }
}

// Register in Program.cs
app.UseMiddleware<LoggingMiddleware>();
```

---

### **✅ 3. Authentication Middleware**
Ensures that only authenticated users can access certain routes.  
```csharp
app.UseAuthentication();
app.UseAuthorization();
```

---

### **✅ 4. Custom Header Middleware**
Adds custom headers to every response.  
```csharp
public class CustomHeaderMiddleware
{
    private readonly RequestDelegate _next;

    public CustomHeaderMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        context.Response.Headers.Add("X-Custom-Header", "Middleware Demo");
        await _next(context);
    }
}

// Register in Program.cs
app.UseMiddleware<CustomHeaderMiddleware>();
```

---

### **✅ 5. Request Timing Middleware**
Measures request execution time.  
```csharp
public class RequestTimingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestTimingMiddleware> _logger;

    public RequestTimingMiddleware(RequestDelegate next, ILogger<RequestTimingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        await _next(context);
        stopwatch.Stop();
        _logger.LogInformation($"Request took {stopwatch.ElapsedMilliseconds} ms");
    }
}

// Register in Program.cs
app.UseMiddleware<RequestTimingMiddleware>();
```

---

### **✅ 6. IP Restriction Middleware**
Blocks requests from certain IP addresses.  
```csharp
public class IpRestrictionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly List<string> _blockedIps = new() { "192.168.1.100" };

    public IpRestrictionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        var ipAddress = context.Connection.RemoteIpAddress?.ToString();
        if (_blockedIps.Contains(ipAddress))
        {
            context.Response.StatusCode = 403;
            await context.Response.WriteAsync("Access Denied.");
            return;
        }

        await _next(context);
    }
}

// Register in Program.cs
app.UseMiddleware<IpRestrictionMiddleware>();
```

---

## **🛠 How to Register Middleware in .NET 9**
Middleware is registered in `Program.cs`. The order of middleware matters.  

```csharp
var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// Exception handling first
app.UseMiddleware<ExceptionHandlingMiddleware>();

// Logging middleware
app.UseMiddleware<LoggingMiddleware>();

// Authentication and Authorization
app.UseAuthentication();
app.UseAuthorization();

// Custom Middleware
app.UseMiddleware<CustomHeaderMiddleware>();
app.UseMiddleware<RequestTimingMiddleware>();

// Endpoint routing
app.UseRouting();
app.UseEndpoints(endpoints => { endpoints.MapControllers(); });

app.Run();
```

---

## **🔥 Conclusion**
- Middleware is **processed in order**, so put error handling **first** and logging early.  
- Use built-in middleware for common tasks like **routing, CORS, and authentication**.  
- Use **custom middleware** for logging, performance tracking, security, and request modifications.  

---







Here’s an **advanced middleware template** with additional functionalities like **IP filtering, rate limiting, and response caching** in .NET 9. 🚀  

---

## **📌 Features in This Middleware Template**  
✅ **Logging** (Request & Response)  
✅ **Exception Handling**  
✅ **IP Filtering** (Block specific IPs)  
✅ **Rate Limiting** (Throttle requests per user)  
✅ **Response Caching** (Cache frequent API responses)  
✅ JWT Authentication (Verify token & extract user identity)
---

# Implementing a Custom Middleware

## **🛠 Full Advanced Middleware Template**
```csharp
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Threading.RateLimiting;

public class AdvancedMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<AdvancedMiddleware> _logger;
    private readonly IMemoryCache _cache;
    private static readonly Dictionary<string, RateLimiter> _rateLimiters = new();
    private readonly string _jwtSecret = "Your_Secret_Key_Here"; // Replace with a secure secret key

    // Blocked IPs list
    private readonly List<string> _blockedIps = new() { "192.168.1.100", "10.0.0.200" };

    public AdvancedMiddleware(RequestDelegate next, ILogger<AdvancedMiddleware> logger, IMemoryCache cache)
    {
        _next = next;
        _logger = logger;
        _cache = cache;
    }

    public async Task Invoke(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        var ipAddress = context.Connection.RemoteIpAddress?.ToString();

        try
        {
            // 🛑 1. IP Filtering: Block specific IPs
            if (_blockedIps.Contains(ipAddress))
            {
                _logger.LogWarning($"Blocked IP: {ipAddress}");
                context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                await context.Response.WriteAsync("Access Denied.");
                return;
            }

            // 🚦 2. Rate Limiting: Allow max 5 requests per minute per IP
            if (!IsRateLimitAllowed(ipAddress))
            {
                context.Response.StatusCode = (int)HttpStatusCode.TooManyRequests;
                await context.Response.WriteAsync("Rate limit exceeded. Try again later.");
                return;
            }

            // 🔑 3. JWT Authentication: Validate token & get user info
            var user = ValidateJwtToken(context);
            if (user == null)
            {
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                await context.Response.WriteAsync("Unauthorized access.");
                return;
            }

            // ⚡ 4. Response Caching: Serve cached responses for GET requests
            if (context.Request.Method == HttpMethods.Get && TryGetCachedResponse(context, out var cachedResponse))
            {
                _logger.LogInformation($"Serving cached response for {context.Request.Path}");
                await context.Response.WriteAsync(cachedResponse);
                return;
            }

            // Log incoming request
            _logger.LogInformation($"Request: {context.Request.Method} {context.Request.Path} from {ipAddress}, User: {user.Identity?.Name}");

            await _next(context); // Call next middleware

            // Store response in cache
            if (context.Request.Method == HttpMethods.Get)
                CacheResponse(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while processing the request.");
            context.Response.StatusCode = 500;
            await context.Response.WriteAsync("An internal error occurred.");
        }
        finally
        {
            stopwatch.Stop();
            _logger.LogInformation($"Request processed in {stopwatch.ElapsedMilliseconds} ms");
        }
    }

    // 🚦 Rate Limiting logic
    private bool IsRateLimitAllowed(string ipAddress)
    {
        if (!_rateLimiters.TryGetValue(ipAddress, out var rateLimiter))
        {
            rateLimiter = new TokenBucketRateLimiter(5, TimeSpan.FromMinutes(1)); // 5 requests per minute
            _rateLimiters[ipAddress] = rateLimiter;
        }
        return rateLimiter.AllowRequest();
    }

    // 🔑 JWT Token Validation
    private ClaimsPrincipal? ValidateJwtToken(HttpContext context)
    {
        var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
        if (string.IsNullOrEmpty(token))
        {
            _logger.LogWarning("No JWT token found.");
            return null;
        }

        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtSecret);
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false,
                ValidateAudience = false
            };

            return tokenHandler.ValidateToken(token, validationParameters, out _);
        }
        catch
        {
            _logger.LogWarning("Invalid JWT token.");
            return null;
        }
    }

    // ⚡ Cache Response logic
    private void CacheResponse(HttpContext context)
    {
        var responseBody = "Cached response for " + context.Request.Path;
        _cache.Set(context.Request.Path, responseBody, TimeSpan.FromMinutes(5)); // Cache for 5 minutes
    }

    private bool TryGetCachedResponse(HttpContext context, out string response)
    {
        return _cache.TryGetValue(context.Request.Path, out response);
    }
}

// Custom Rate Limiter Class (Token Bucket Algorithm)
public class TokenBucketRateLimiter
{
    private int _tokens;
    private readonly int _maxTokens;
    private DateTime _lastRefill;
    private readonly TimeSpan _refillTime;

    public TokenBucketRateLimiter(int maxTokens, TimeSpan refillTime)
    {
        _maxTokens = maxTokens;
        _refillTime = refillTime;
        _tokens = maxTokens;
        _lastRefill = DateTime.UtcNow;
    }

    public bool AllowRequest()
    {
        var now = DateTime.UtcNow;
        var elapsed = now - _lastRefill;

        if (elapsed >= _refillTime)
        {
            _tokens = _maxTokens;
            _lastRefill = now;
        }

        if (_tokens > 0)
        {
            _tokens--;
            return true;
        }
        return false;
    }
}

// Register Middleware in Program.cs
public static class MiddlewareExtensions
{
    public static IApplicationBuilder UseAdvancedMiddleware(this IApplicationBuilder app)
    {
        return app.UseMiddleware<AdvancedMiddleware>();
    }
}

```

---

## **🛠 How to Use This Middleware in .NET 9**
1. **Install Memory Cache Dependency**  
   Add `IMemoryCache` service in `Program.cs`:  
   ```csharp
   var builder = WebApplication.CreateBuilder(args);
   builder.Services.AddMemoryCache(); // Required for caching
   builder.Services.AddAuthentication();
   var app = builder.Build();
   ```
2. **Register Middleware in `Program.cs`**
   ```csharp
   app.UseAdvancedMiddleware(); // Register custom middleware
   ```

---

## **📌 What This Middleware Does**
🔹 **Blocks unwanted IP addresses**  
🔹 **Limits users to 5 requests per minute** (Rate Limiting)  
🔹 **Caches GET responses for 5 minutes**  
🔹 **Logs request details & execution time**  
🔹 **Handles exceptions gracefully**  
🔹 Validates JWT tokens for secure authentication

---

## **🔥 Why This is Useful**

✔ **Secures API endpoints with JWT authentication**\
✔ **Prevents DDoS & spam attacks**  
✔ **Improves API response times with caching**  
✔ **Ensures fair API usage with rate limiting**  
✔ **Enhances security by blocking malicious IPs**  

Would you like more enhancements, such as **JWT Authentication support or API Key validation**? 🚀