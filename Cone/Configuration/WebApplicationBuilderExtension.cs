using System.Net;
using System.Security.Claims;
using Cone.Db;
using Cone.Db.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;

namespace Cone.Configuration;

public static class WebApplicationBuilderExtension
{
    extension(WebApplicationBuilder builder)
    {
        public WebApplicationBuilder AddConeConfiguration()
        {
            builder.Services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto |
                                           ForwardedHeaders.XForwardedHost | ForwardedHeaders.XForwardedPrefix;
                builder.Configuration.GetSection("ForwardedHeaders")
                    .GetSection("KnownNetworks")
                    .Get<string[]>()?
                    .Select(System.Net.IPNetwork.Parse)
                    .ToList()
                    .ForEach(x => options.KnownIPNetworks.Add(x));

                builder.ConfigureTrustCloudFlareProxy(options);
            
                options.ForwardLimit = null;
            });
        
            builder.Services.AddControllersWithViews();
            builder.Services.AddRazorPages();

            // Add DB configuration
            builder.AddConeDbConfiguration();

            // Add custom services
            builder.Services.AddConeServices(builder.Configuration);

            // Configure Sentry
            builder.ConfigureSentry();

            // Add Google Authentication
            builder.AddGoogleAuthentication();

            return builder;
        }

        private WebApplicationBuilder AddConeDbConfiguration()
        {
            var dbConnectionString = builder.Configuration.GetConnectionString("DbConnection")
                                     ?? throw new InvalidOperationException("DB ConnectionString must not be null.");

            builder.Services.AddDbContext<ConeDbContext>(options =>
                options.UseNpgsql(dbConnectionString)
            );

            // Register IConeDbContext to resolve to ConeDbContext
            builder.Services.AddScoped<IConeDbContext>(provider =>
                provider.GetRequiredService<ConeDbContext>());

            return builder;
        }

        private WebApplicationBuilder ConfigureSentry()
        {
            var dsn = builder.Configuration.GetValue<string>("Sentry:Dsn");
            var isTargetEnvironment = builder.Environment.IsProduction() ||
                                      builder.Environment.IsStaging();

            if (isTargetEnvironment && string.IsNullOrWhiteSpace(dsn))
            {
                throw new InvalidOperationException("Sentry DSN is not configured.");
            }

            builder.WebHost.UseSentry(o =>
            {
                o.Dsn = dsn ?? string.Empty;
                o.Debug = builder.Environment.IsDevelopment();
            });

            return builder;
        }

        private WebApplicationBuilder AddGoogleAuthentication()
        {
            var googleClientId = builder.Configuration.GetValue<string>("GoogleAuth:ClientId");
            var googleClientSecret =
                builder.Configuration.GetValue<string>("GoogleAuth:ClientSecret");

            if (string.IsNullOrWhiteSpace(googleClientId) || string.IsNullOrWhiteSpace(googleClientSecret))
            {
                throw new InvalidOperationException("Google Authentication is not configured properly.");
            }

            builder.Services.AddAuthentication(options =>
                {
                    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
                })
                .AddCookie(options =>
                {
                    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                    options.AccessDeniedPath = "/error/403";
                })
                .AddGoogle(options =>
                {
                    options.ClientId = googleClientId;
                    options.ClientSecret = googleClientSecret;
                
                    options.CorrelationCookie.SecurePolicy = CookieSecurePolicy.Always;
                    options.CallbackPath = "/signin-google";
                });

            // 管理者ポリシー登録
            builder.Services.AddAuthorizationBuilder()
                .AddPolicy("Admin", policyBuilder => { policyBuilder.Requirements.Add(new CustomAdminRequirement()); });

            // 特定のドメインでのアクセスを許可するポリシーの追加
            var allowedDomains = builder.Configuration.GetSection("AllowedEmailEndPrefixes").Get<string[]>();
            if (allowedDomains == null || allowedDomains.Length == 0)
            {
                throw new InvalidOperationException("AllowedEmailEndPrefixes is not configured.");
            }

            builder.Services.AddAuthorizationBuilder()
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

            builder.Services.AddCascadingAuthenticationState();

            return builder;
        }

        private WebApplicationBuilder ConfigureTrustCloudFlareProxy(ForwardedHeadersOptions options
        )
        {
            var urls = new[] { "https://www.cloudflare.com/ips-v4", "https://www.cloudflare.com/ips-v6" };

            using var httpClient = new HttpClient();

            var allNetworks = new List<System.Net.IPNetwork>();

            foreach (var url in urls)
            {
                try
                {
                    using var webRequest = new HttpRequestMessage(HttpMethod.Get, url);
                    using var response = httpClient.Send(webRequest);
                    response.EnsureSuccessStatusCode();
                    using var reader = new StreamReader(response.Content.ReadAsStream());
                    var text = reader.ReadToEnd();

                    var networks = text
                        .Split('\n', StringSplitOptions.RemoveEmptyEntries)
                        .Select(line => line.Trim())
                        .Where(line => !string.IsNullOrWhiteSpace(line) && !line.StartsWith($"#"))
                        .Select(System.Net.IPNetwork.Parse);

                    allNetworks.AddRange(networks);
                }
                catch (System.Exception ex)
                {
                    throw new InvalidOperationException($"Failed to fetch Cloudflare IPs from {url}", ex);
                }
            }

            allNetworks.ForEach(x => options.KnownIPNetworks.Add(x));

            return builder;
        }
    }
}