using System.Net.Mime;
using System.Reflection;
using Ardalis.GuardClauses;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.OpenApi.Models;
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

        services.AddSwagger();
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

    /// <summary>
    /// Adds swagger configuration.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>An instance of <see cref="IServiceCollection"/>.</returns>
    private static void AddSwagger(this IServiceCollection services)
    {
        Guard.Against.Null(services);

        services.AddSwaggerGen(options =>
        {
#pragma warning disable S1075 // URIs should not be hardcoded
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Version = "v1",
                Title = "Planning Poker API",
                Description = "API for managing scrum poker tables.",
                TermsOfService = new Uri("https://placeholder/terms"),
                Contact = new OpenApiContact
                {
                    Name = "Vasil Kotsev",
                    Url = new Uri("https://placeholder.vk")
                },
                License = new OpenApiLicense
                {
                    Name = "MIT Licence",
                    Url = new Uri("https://github.com/SonnyRR/planning-poker/blob/master/LICENSE")
                }
            });
#pragma warning restore S1075 // URIs should not be hardcoded

            var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
        });
    }
}
