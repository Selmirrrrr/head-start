using System.Net.Http.Headers;
using System.Net.Mime;
using Ardalis.GuardClauses;
using HeadStart.BFF.Utilities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Mvc;
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

    public static void AddApiServices(this IServiceCollection services, IConfiguration configuration, bool isDevelopment)
    {
        Guard.Against.Null(services);
        Guard.Against.Null(configuration);

        services.AddAntiforgeryConfiguration();
        services.AddHttpClient();
        services.AddOptions();

        services.AddResponseCompression(opts =>
            opts.MimeTypes = new List<string>(ResponseCompressionDefaults.MimeTypes)
            {
                MediaTypeNames.Application.Octet
            }
        );

        services.AddReverseProxyConfiguration(configuration);
        services.AddAuthentication(configuration, isDevelopment);
        services.AddAuthorization();
        services.AddSignalR();
        services.AddRazorPages();
        services.AddControllersWithViews(options => options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute()));
        services.AddTransient<IClaimsTransformation, ClaimsTransformer>();
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
            .AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, options =>
            {
                options.Authority = configuration["OpenIDConnectSettings:Authority"] ?? "http://localhost:8080/realms/HeadStart";
                options.ClientId = "HeadStartWeb";
                options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.ResponseType = OpenIdConnectResponseType.Code;

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

        services.AddReverseProxy()
            .LoadFromConfig(configuration.GetSection("ReverseProxy"))
            .AddTransforms(builder => builder.AddRequestTransform(async context =>
            {
                // Attach the access token retrieved from the authentication cookie.
                //
                // Note: in a real world application, the expiration date of the access token
                // should be checked before sending a request to avoid getting a 401 response.
                // Once expired, a new access token could be retrieved using the OAuth 2.0
                // refresh token grant (which could be done transparently).
                var token = await context.HttpContext.GetTokenAsync("access_token");

                context.ProxyRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }));
    }
}
