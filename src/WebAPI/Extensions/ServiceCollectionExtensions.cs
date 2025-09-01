using System.Net.Mime;
using System.Security.Claims;
using Ardalis.GuardClauses;
using FastEndpoints;
using FastEndpoints.Swagger;
using HeadStart.WebAPI.Data;
using HeadStart.WebAPI.Data.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using NSwag;

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
                    s.AddSecurity("OAuth2", [], new OpenApiSecurityScheme
                    {
                        Type = OpenApiSecuritySchemeType.OAuth2,
                        Description = "OAuth2 Keyycloak",
                        Flow = OpenApiOAuth2Flow.Password,
                        Flows = new OpenApiOAuthFlows()
                        {
                            Password = new OpenApiOAuthFlow()
                            {
                                AuthorizationUrl = "http://localhost:8080/realms/HeadStart/protocol/openid-connect/auth",
                                TokenUrl = "http://localhost:8080/realms/HeadStart/protocol/openid-connect/token"
                            }
                        }
                    });
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
                        var roleAdminId = Guid.CreateVersion7();
                        var roleUserId = Guid.CreateVersion7();
                        context.Set<Tenant>().Add(new Tenant { Path = "HeadStart", Name = "HeadStart" });
                        context.Set<Tenant>().Add(new Tenant { Path = "HeadStart.Lausanne", Name = "HeadStart Lausanne" });
                        context.Set<Tenant>().Add(new Tenant { Path = "HeadStart.Zürich", Name = "HeadStart Zürich" });
                        context.Set<Tenant>().Add(new Tenant { Path = "HeadStart.Lausanne.Palud", Name = "HeadStart Lausanne - Palud" });
                        context.Set<Tenant>().Add(new Tenant { Path = "HeadStart.Lausanne.Ouchy", Name = "HeadStart Lausanne - Ouchy" });
                        context.Set<Tenant>().Add(new Tenant { Path = "HeadStart.Zürich.Paradeplatz", Name = "HeadStart Zürich - Paradeplatz" });
                        context.Set<Tenant>().Add(new Tenant { Path = "HeadStart.Zürich.Enge", Name = "HeadStart Zürich - Enge" });
                        context.Set<Role>().Add(new Role { Id = roleAdminId, Code = "Admin", CodeTrads = new Dictionary<string, string> { { "fr", "Administrateur" }, { "de", "Administrator" }, { "it", "Amministratore" }, { "en", "Administrator" } }, TenantPath = "HeadStart" });
                        context.Set<Role>().Add(new Role { Id = roleUserId, Code = "User", CodeTrads = new Dictionary<string, string> { { "fr", "Utilisteur" }, { "de", "Benutzer" }, { "it", "Utilizatore" }, { "en", "User" } }, TenantPath = "HeadStart" });
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
