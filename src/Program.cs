using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using System;
using Microsoft.AspNetCore.Http;
using Serilog;
using Microsoft.Extensions.Configuration;


try
{
        var builder = WebApplication.CreateBuilder(args).ConfigureApplicationBuilder();

        var app = builder.Build();

        app.ConfigureApplication();

        // Log.Information($"Application is running in {builder.Environment.EnvironmentName} mode.");

        app.Run();
        
        return 0;
}
catch (Exception ex)
{
        Log.Fatal(ex, "Host terminated unexpectedly");
        return 1;
}
finally
{
        Log.CloseAndFlush();
}