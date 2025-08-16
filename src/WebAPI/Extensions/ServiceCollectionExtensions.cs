using System.Net.Mime;
using Ardalis.GuardClauses;
using Microsoft.AspNetCore.ResponseCompression;
using OpenIddict.Validation.AspNetCore;

namespace HeadStart.WebAPI.Extensions;

/// <summary>
/// Contains extension methods for registering application services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds all API service configurations.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The web host configuration.</param>
    /// <returns>An instance of <see cref="IServiceCollection"/>.</returns>
    public static void AddApiServices(this IServiceCollection services, IConfiguration configuration)
    {
        Guard.Against.Null(services);
        Guard.Against.Null(configuration);

        services.AddOidcServices();
        services.AddControllers();
        services.AddResponseCompression(opts =>
            opts.MimeTypes = new List<string>(ResponseCompressionDefaults.MimeTypes)
            {
                MediaTypeNames.Application.Octet
            }
        );

        services.AddSignalR();
        services.AddHttpContextAccessor();
    }

    /// <summary>
    /// Adds OIDC configuration.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>An instance of <see cref="IServiceCollection"/>.</returns>
    private static void AddOidcServices(this IServiceCollection services)
    {
        Guard.Against.Null(services);

        using var serviceProvider = services.BuildServiceProvider();
        var configuration = serviceProvider.GetRequiredService<IConfiguration>();

        Guard.Against.Null(configuration);

        var authority = configuration["OidcConfiguration:Authority"];
        Guard.Against.NullOrWhiteSpace(authority);

        var clientSecret = configuration["OidcConfiguration:ClientSecret"];
        Guard.Against.NullOrWhiteSpace(clientSecret);

        // Register the OpenIddict validation components.
        services.AddOpenIddict()
            .AddValidation(options =>
            {
                // Note: the validation handler uses OpenID Connect discovery
                // to retrieve the address of the introspection endpoint.
                options.SetIssuer(authority);

                // Configure audience validation
                options.AddAudiences("headstart.api");

                // Configure the introspection endpoint.
                options.UseIntrospection().SetClientId("headstart.api").SetClientSecret(clientSecret);

                // Register the System.Net.Http integration.
                options.UseSystemNetHttp();

                // Register the ASP.NET Core host.
                options.UseAspNetCore();
            });

        services.AddAuthentication(OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme);
        services.AddAuthorization();
    }
}
