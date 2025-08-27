using System.Net.Mime;
using Ardalis.GuardClauses;
using FastEndpoints;
using FastEndpoints.Swagger;
using HeadStart.WebAPI.Data;
using HeadStart.WebAPI.Data.Models;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;

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
                o.MaxEndpointVersion = 1;
                o.DocumentSettings = s =>
                {
                    s.Title = "HeadStart API";
                    s.Version = "v1";
                    s.DocumentName = "headstart-api-v1";
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
                        context.Set<Tenant>().Add(new Tenant { Path = "HeadStart", Name = "HeadStart" });
                        context.Set<Tenant>().Add(new Tenant { Path = "HeadStart.Lausanne", Name = "HeadStart Lausanne" });
                        context.Set<Tenant>().Add(new Tenant { Path = "HeadStart.Zürich", Name = "HeadStart Zürich" });
                        context.Set<Tenant>().Add(new Tenant { Path = "HeadStart.Lausanne.Palud", Name = "HeadStart Lausanne - Palud" });
                        context.Set<Tenant>().Add(new Tenant { Path = "HeadStart.Lausanne.Ouchy", Name = "HeadStart Lausanne - Ouchy" });
                        context.Set<Tenant>().Add(new Tenant { Path = "HeadStart.Zürich.Paradeplatz", Name = "HeadStart Zürich - Paradeplatz" });
                        context.Set<Tenant>().Add(new Tenant { Path = "HeadStart.Zürich.Enge", Name = "HeadStart Zürich - Enge" });
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
    /// Adds Auth.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="isDevelopment">Defines if we're in dev mode</param>
    /// <returns>An instance of <see cref="IServiceCollection"/>.</returns>
    internal static void AddAuth(this IServiceCollection services, bool isDevelopment)
    {
        Guard.Against.Null(services);

        services.AddAuthentication()
            .AddKeycloakJwtBearer("keycloak", realm: "HeadStart", options =>
            {
                if (isDevelopment)
                {
                    options.RequireHttpsMetadata = false;
                }
                options.Audience = "headstart.api";
            });
        services.AddAuthorizationBuilder();
    }
}
