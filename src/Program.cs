using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using System;
using Microsoft.AspNetCore.Http;
using Serilog;
using Microsoft.Extensions.Configuration;


var builder = WebApplication
                .CreateBuilder(args)
                .ConfigureApplicationBuilder();

var IsDevelopment = builder.Environment.IsDevelopment();

var configuration = builder.Configuration;

var app = builder
        .Build()
        .ConfigureApplication();

try
{

        // Console.WriteLine(app.Environment.IsDevelopment().ToString());
        Log.Information($"Starting host in {builder.Environment.EnvironmentName} mode");
        // Console.WriteLine($"Running in Dev={isDev} mode");

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