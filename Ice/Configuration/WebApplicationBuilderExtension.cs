using System.Security.Claims;
using Ice.Db;
using Ice.Db.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;

namespace Ice.Configuration;

public static class WebApplicationBuilderExtension
{
    public static WebApplicationBuilder AddIceConfiguration(this WebApplicationBuilder builder)
    {
        builder.Services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto |
                                       ForwardedHeaders.XForwardedHost | ForwardedHeaders.XForwardedPrefix;
            builder.Configuration.GetSection("ForwardedHeaders")
                .GetSection("KnownNetworks")
                .Get<string[]>()?
                .Select(x => IPNetwork.Parse(x))
                .ToList()
                .ForEach(x => options.KnownNetworks.Add(x));
            
            options.ForwardLimit = null;
        });
        
        builder.Services.AddControllersWithViews();
        builder.Services.AddRazorPages();

        // Add DB configuration
        builder.AddIceDbConfiguration();

        // Add custom services
        builder.Services.AddIceServices(builder.Configuration);

        // Configure Sentry
        builder.ConfigureSentry();

        // Add Google Authentication
        builder.AddGoogleAuthentication();

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
        webApplicationBuilder.Services.AddScoped<IIceDbContext>(provider =>
            provider.GetRequiredService<IceDbContext>());

        return webApplicationBuilder;
    }

    private static WebApplicationBuilder ConfigureSentry(this WebApplicationBuilder webApplicationBuilder)
    {
        var dsn = webApplicationBuilder.Configuration.GetValue<string>("Sentry:Dsn");
        var isTargetEnvironment = webApplicationBuilder.Environment.IsProduction() ||
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

    private static WebApplicationBuilder AddGoogleAuthentication(this WebApplicationBuilder webApplicationBuilder)
    {
        var googleClientId = webApplicationBuilder.Configuration.GetValue<string>("GoogleAuth:ClientId");
        var googleClientSecret =
            webApplicationBuilder.Configuration.GetValue<string>("GoogleAuth:ClientSecret");

        if (string.IsNullOrWhiteSpace(googleClientId) || string.IsNullOrWhiteSpace(googleClientSecret))
        {
            throw new InvalidOperationException("Google Authentication is not configured properly.");
        }

        webApplicationBuilder.Services.AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
            })
            .AddCookie(options =>
            {
                options.AccessDeniedPath = "/error/403";
            })
            .AddGoogle(options =>
            {
                options.ClientId = googleClientId;
                options.ClientSecret = googleClientSecret;
            });

        // 管理者ポリシー登録
        webApplicationBuilder.Services.AddAuthorizationBuilder()
            .AddPolicy("Admin", policyBuilder => { policyBuilder.Requirements.Add(new CustomAdminRequirement()); });

        // 特定のドメインでのアクセスを許可するポリシーの追加
        var allowedDomains = webApplicationBuilder.Configuration.GetSection("AllowedEmailEndPrefixes").Get<string[]>();
        if (allowedDomains == null || allowedDomains.Length == 0)
        {
            throw new InvalidOperationException("AllowedEmailEndPrefixes is not configured.");
        }

        webApplicationBuilder.Services.AddAuthorizationBuilder()
            .AddPolicy("AllowedEmailDomain",
                policyBuilder =>
                {
                    policyBuilder.RequireAssertion(context =>
                    {
                        var email = context.User.FindFirstValue(ClaimTypes.Email);
                        if (string.IsNullOrEmpty(email)) return false;
                        return allowedDomains.Any(domain =>
                            email.EndsWith("@" + domain, StringComparison.OrdinalIgnoreCase));
                    });
                });

        webApplicationBuilder.Services.AddCascadingAuthenticationState();

        return webApplicationBuilder;
    }
}