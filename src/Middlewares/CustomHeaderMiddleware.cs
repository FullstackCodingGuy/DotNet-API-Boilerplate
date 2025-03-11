public class CustomHeaderMiddleware
{
    private readonly RequestDelegate _next;

    public CustomHeaderMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        context.Response.Headers["X-Custom-Header"] = "MyCustomHeaderValue";
        await _next(context);
    }
}

// Register in Program.cs
// app.UseMiddleware<CustomHeaderMiddleware>();