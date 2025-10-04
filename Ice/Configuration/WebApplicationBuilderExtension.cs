using Ice.Db;
using Microsoft.EntityFrameworkCore;

namespace Ice.Configuration;

public static class WebApplicationBuilderExtension
{
    public static WebApplicationBuilder AddIceConfiguration(this WebApplicationBuilder builder)
    {
        builder.Services.AddControllersWithViews();
        builder.Services.AddRazorPages();
        
        // Add DB configuration
        builder.AddIceDbConfiguration();

        // Add custom services
        builder.Services.AddIceServices(builder.Configuration);

        // Configure Sentry
        builder.ConfigureSentry();

        return builder;
    }
    
    private static WebApplicationBuilder AddIceDbConfiguration(this WebApplicationBuilder webApplicationBuilder)
    {
        var dbConnectionString = webApplicationBuilder.Configuration.GetConnectionString("DbConnection")
                                 ?? throw new InvalidOperationException("DB ConnectionString must not be null.");

        webApplicationBuilder.Services.AddDbContext<IceDbContext>(options =>
            options.UseNpgsql(dbConnectionString)
        );
        
        // Register IIceDbContext to resolve to IceDbContext
        webApplicationBuilder.Services.AddScoped<IIceDbContext>(provider => provider.GetRequiredService<IceDbContext>());

        return webApplicationBuilder;
    }
    
    private static WebApplicationBuilder ConfigureSentry(this WebApplicationBuilder webApplicationBuilder)
    {
        var dsn = webApplicationBuilder.Configuration.GetValue<string>("Sentry:Dsn");
        var isTargetEnvironment = webApplicationBuilder.Environment.IsDevelopment() ||
                                  webApplicationBuilder.Environment.IsProduction() ||
                                  webApplicationBuilder.Environment.IsStaging();

        if (isTargetEnvironment && string.IsNullOrWhiteSpace(dsn))
        {
            throw new InvalidOperationException("Sentry DSN is not configured.");
        }

        webApplicationBuilder.WebHost.UseSentry(o =>
        {
            o.Dsn = dsn ?? string.Empty;
            o.Debug = webApplicationBuilder.Environment.IsDevelopment();
        });

        return webApplicationBuilder;
    }
}