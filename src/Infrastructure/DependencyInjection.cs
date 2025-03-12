namespace NetAPI.Infrastructure;

using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Microsoft.AspNetCore.Builder;
using Serilog;

[ExcludeFromCodeCoverage]
public static class DependencyInjection
{
    public static IServiceCollection AddInfra(this IServiceCollection services)
    {
        // var IsDevelopment = app.Environment.IsDevelopment();


        return services;
    }

}