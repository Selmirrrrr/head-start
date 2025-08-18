using System.Net.Mime;
using Ardalis.GuardClauses;
using FastEndpoints;
using FastEndpoints.Swagger;
using HeadStart.WebAPI.Data;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using OpenIddict.Validation.AspNetCore;
using Scalar.AspNetCore;

namespace HeadStart.WebAPI.Extensions;

/// <summary>
/// Contains extension methods for registering application services.
/// </summary>
internal static class ServiceCollectionExtensions
{
    internal static void AddApiFramework(this IServiceCollection services)
    {
        Guard.Against.Null(services);

        services.AddFastEndpoints()
            .SwaggerDocument(o =>
            {
                o.DocumentSettings = s =>
                {
                    s.Title = "HeadStart API";
                    s.Version = "v1";
                    s.DocumentName = "HeadStartAPIv1";
                };
            });

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

    internal static void AddDatabaseServices(this IServiceCollection services, IConfiguration configuration)
    {
        Guard.Against.Null(services);
        Guard.Against.Null(configuration);

        services.AddDbContext<HeadStartDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("postgresdb") ?? throw new InvalidOperationException("Connection string 'postgresdb' not found."))
                .UseAsyncSeeding(async (context, _, cancellationToken) =>
                {
                    var testBlog = await context.Set<Tenant>().FirstOrDefaultAsync(cancellationToken);
                    if (testBlog == null)
                    {
                        context.Set<Tenant>().Add(new Tenant { Id = Guid.NewGuid(), Name = "HeadStart" });
                        await context.SaveChangesAsync(cancellationToken);
                    }
                }));
    }

    internal static void AddSecurityServices(this IServiceCollection services)
    {
        Guard.Against.Null(services);

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

    /// <summary>
    /// Adds OIDC configuration.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>An instance of <see cref="IServiceCollection"/>.</returns>
    internal static void AddOidcServices(this IServiceCollection services)
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
