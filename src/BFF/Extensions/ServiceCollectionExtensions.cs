using System.Net.Http.Headers;
using System.Net.Mime;
using System.Threading.RateLimiting;
using Ardalis.GuardClauses;
using Duende.AccessTokenManagement.OpenIdConnect;
using HeadStart.BFF.Utilities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Yarp.ReverseProxy.Transforms;

namespace HeadStart.BFF.Extensions;

public static class ServiceCollectionExtensions
{
    private static void AddAntiforgeryConfiguration(this IServiceCollection services)
    {
        Guard.Against.Null(services);

        services.AddAntiforgery(options =>
        {
            options.HeaderName = "X-XSRF-TOKEN";
            options.Cookie.Name = "__Host-X-XSRF-TOKEN";
            options.Cookie.SameSite = SameSiteMode.Strict;
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        });
    }

    public static void AddBffServices(this IServiceCollection services, IConfiguration configuration, bool isDevelopment)
    {
        Guard.Against.Null(services);
        Guard.Against.Null(configuration);

        services.AddWebServices();
        services.AddSecurityServices();
        services.AddRateLimitingServices();
        services.AddReverseProxyConfiguration(configuration);
        services.AddAuthentication(configuration, isDevelopment);
        services.AddAuthorization();
        services.AddTransient<IClaimsTransformation, ClaimsTransformer>();
    }

    private static void AddWebServices(this IServiceCollection services)
    {
        Guard.Against.Null(services);

        services.AddHttpClient();
        services.AddOptions();
        services.AddSignalR();
        services.AddRazorPages();
        services.AddControllersWithViews(options => options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute()));

        services.AddResponseCompression(opts =>
            opts.MimeTypes = new List<string>(ResponseCompressionDefaults.MimeTypes)
            {
                MediaTypeNames.Application.Octet
            }
        );
    }

    private static void AddSecurityServices(this IServiceCollection services)
    {
        Guard.Against.Null(services);

        services.AddAntiforgeryConfiguration();
        services.AddDataProtection(o => o.ApplicationDiscriminator = "HeadStart");
        services.AddProblemDetails(options =>
        {
            options.CustomizeProblemDetails = context =>
            {
                context.ProblemDetails.Instance = $"{context.HttpContext.Request.Method} {context.HttpContext.Request.Path}";
                context.ProblemDetails.Extensions.TryAdd("requestId", context.HttpContext.TraceIdentifier);
                var activity = context.HttpContext.Features.Get<IHttpActivityFeature>()?.Activity;
                if (activity != null)
                {
                    context.ProblemDetails.Extensions.TryAdd("traceId", activity.Id);
                }
            };
        });
    }

    private static void AddRateLimitingServices(this IServiceCollection services)
    {
        Guard.Against.Null(services);

        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            // Add rate limiter for authentication endpoints
            options.AddFixedWindowLimiter("auth", limiterOptions =>
            {
                limiterOptions.Window = TimeSpan.FromSeconds(30);
                limiterOptions.PermitLimit = 5;
                limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                limiterOptions.QueueLimit = 2;
            });

            // Add a stricter rate limiter for login attempts
            options.AddFixedWindowLimiter("login", limiterOptions =>
            {
                limiterOptions.Window = TimeSpan.FromSeconds(30);
                limiterOptions.PermitLimit = 10;
                limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                limiterOptions.QueueLimit = 0;
            });
        });
    }

    private static void AddAuthentication(this IServiceCollection services, IConfiguration configuration, bool isDevelopment)
    {
        Guard.Against.Null(services);

        using var serviceProvider = services.BuildServiceProvider();

        services.AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
            })
            .AddCookie()
            .AddKeycloakOpenIdConnect(
                serviceName: "keycloak",
                realm: "HeadStart",
                options =>
                {
                    options.ClientId = "HeadStartWeb";
                    options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    options.ResponseType = OpenIdConnectResponseType.Code;
                    // Request refresh token
                    options.Scope.Add("offline_access");

                    options.SaveTokens = true;
                    options.GetClaimsFromUserInfoEndpoint = true;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        NameClaimType = "name",
                        RoleClaimType = "role"
                    };

                    // For development only - disable HTTPS metadata validation
                    // In production, use explicit Authority configuration instead
                    if (isDevelopment)
                    {
                        options.RequireHttpsMetadata = false;
                    }
                });

        // Add Duende automatic token management
        services.AddOpenIdConnectAccessTokenManagement();
    }

    private static void AddAuthorization(this IServiceCollection services)
    {
        Guard.Against.Null(services);

        // Create an authorization policy used by YARP when forwarding requests
        services.AddAuthorization(options => options.AddPolicy("CookieAuthenticationPolicy", builder =>
        {
            builder.AddAuthenticationSchemes(CookieAuthenticationDefaults.AuthenticationScheme);
            builder.RequireAuthenticatedUser();
        }));
    }

    private static void AddReverseProxyConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        Guard.Against.Null(services);
        Guard.Against.Null(configuration);

        // Validate that ReverseProxy configuration section exists
        var reverseProxySection = configuration.GetSection("ReverseProxy");
        if (!reverseProxySection.Exists())
        {
            throw new InvalidOperationException("ReverseProxy configuration section is missing from appsettings.json");
        }

        services.AddReverseProxy()
            .LoadFromConfig(reverseProxySection)
            .AddServiceDiscoveryDestinationResolver()
            .AddTransforms(builder => builder.AddRequestTransform(async context =>
            {
                // Use Duende's automatic token management
                var tokenManagementService = context.HttpContext.RequestServices.GetRequiredService<IUserTokenManager>();
                var tokenResult = await tokenManagementService.GetAccessTokenAsync(context.HttpContext.User);

                if (!tokenResult.Succeeded)
                {
                    // Token retrieval failed, user needs to authenticate
                    context.HttpContext.Response.StatusCode = 401;
                    context.HttpContext.Response.Headers["WWW-Authenticate"] = "Bearer";
                    return;
                }

                // Token is valid (Duende handles refresh automatically), attach it to the proxy request
                context.ProxyRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokenResult.Token.AccessToken);
            }));
    }
}
