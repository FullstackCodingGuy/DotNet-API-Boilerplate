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
        if (ipAddress != null && _blockedIps.Contains(ipAddress))
        {
            context.Response.StatusCode = 403;
            await context.Response.WriteAsync("Access Denied.");
            return;
        }

        await _next(context);
    }
}

// Register in Program.cs
// app.UseMiddleware<IpRestrictionMiddleware>();