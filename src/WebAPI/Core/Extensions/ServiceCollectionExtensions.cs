using System.Net.Mime;
using Ardalis.GuardClauses;
using FastEndpoints;
using FastEndpoints.Swagger;
using HeadStart.WebAPI.Data;
using HeadStart.WebAPI.Data.Models;
using HeadStart.WebAPI.Services;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using NSwag;

namespace HeadStart.WebAPI.Core.Extensions;

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
        services.AddScoped<CurrentUserService>();
    }

    internal static void AddDatabaseServices(this IServiceCollection services, IConfiguration configuration)
    {
        Guard.Against.Null(services);
        Guard.Against.Null(configuration);

        services.AddDbContext<HeadStartDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("postgresdb") ?? throw new InvalidOperationException("Connection string 'postgresdb' not found."))
                .UseAsyncSeeding(async (context, _, cancellationToken) =>
                {
                    var tenant = await context.Set<Tenant>().FirstOrDefaultAsync(cancellationToken);
                    if (tenant == null)
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


                        var superAdmin = context.Set<Utilisateur>().Add(new Utilisateur { Id = Guid.CreateVersion7(), IdpId = Guid.Parse("05623570-9015-4149-A52E-B01975772D32"), Email = "superadmin@headstart.com", Nom = "Super", Prenom = "Admin", LanguageCode = "fr", DarkMode = false });

                        var userApiTest1 = context.Set<Utilisateur>().Add(new Utilisateur { Id = Guid.CreateVersion7(), IdpId = Guid.Parse("A599B326-C0BF-4F29-91CF-463ADA378253"), Email = "user.api.1@test.com", Nom = "UserApiTest1N", Prenom = "UserApiTest1P", LanguageCode = "fr", DarkMode = false, DernierTenantSelectionneId = "HeadStart" });
                        var userApiTest2 = context.Set<Utilisateur>().Add(new Utilisateur { Id = Guid.CreateVersion7(), IdpId = Guid.Parse("950ED1AF-A46B-4E59-9C37-C58E3AB50CCE"), Email = "user.api.2@test.com", Nom = "UserApiTest2N", Prenom = "UserApiTest2P", LanguageCode = "fr", DarkMode = false, DernierTenantSelectionneId = "HeadStart" });
                        var userApiTest3 = context.Set<Utilisateur>().Add(new Utilisateur { Id = Guid.CreateVersion7(), IdpId = Guid.Parse("68A6EB54-7EF3-4355-9F5D-60617C5DB25D"), Email = "user.api.3@test.com", Nom = "UserApiTest3N", Prenom = "UserApiTest3P", LanguageCode = "fr", DarkMode = false, DernierTenantSelectionneId = "HeadStart" });

                        var userUiTest1 = context.Set<Utilisateur>().Add(new Utilisateur { Id = Guid.CreateVersion7(), IdpId = Guid.Parse("51DC8821-CED1-434B-9D35-22A1B6BD6080"), Email = "user.ui.1@test.com", Nom = "UserUiTest1N", Prenom = "UserUiTest1P", LanguageCode = "fr", DarkMode = false, DernierTenantSelectionneId = "HeadStart" });

                        var adminApiTest1 = context.Set<Utilisateur>().Add(new Utilisateur { Id = Guid.CreateVersion7(), IdpId = Guid.Parse("1CD0057D-F96B-4D6F-A2FF-393C1CFEF71C"), Email = "admin.api.1@test.com", Nom = "AdminApiTest1N", Prenom = "AdminApiTest1P", LanguageCode = "fr", DarkMode = false, DernierTenantSelectionneId = "HeadStart" });
                        var adminApiTest2 = context.Set<Utilisateur>().Add(new Utilisateur { Id = Guid.CreateVersion7(), IdpId = Guid.Parse("BEF49FA1-E260-4F14-9DA8-4B3E8B1AEED9"), Email = "admin.api.2@test.com", Nom = "AdminApiTest2N", Prenom = "AdminApiTest2P", LanguageCode = "fr", DarkMode = false, DernierTenantSelectionneId = "HeadStart" });
                        var adminApiTest3 = context.Set<Utilisateur>().Add(new Utilisateur { Id = Guid.CreateVersion7(), IdpId = Guid.Parse("62C43E2A-BA99-442A-8FB0-74F78EA29D3E"), Email = "admin.api.3@test.com", Nom = "AdminApiTest3N", Prenom = "AdminApiTest3P", LanguageCode = "fr", DarkMode = false, DernierTenantSelectionneId = "HeadStart" });

                        var adminUiTest1 = context.Set<Utilisateur>().Add(new Utilisateur { Id = Guid.CreateVersion7(), IdpId = Guid.Parse("AD9B256F-6541-485E-B17B-6451449AD980"), Email = "admin.ui.1@test.com", Nom = "AdminUiTest1N", Prenom = "AdminUiTest1P", LanguageCode = "fr", DarkMode = false, DernierTenantSelectionneId = "HeadStart" });

                        context.Set<Droit>().Add(Droit.New(userApiTest1.Entity.Id, "HeadStart", roleUserId));
                        context.Set<Droit>().Add(Droit.New(userApiTest2.Entity.Id, "HeadStart", roleUserId));
                        context.Set<Droit>().Add(Droit.New(userApiTest3.Entity.Id, "HeadStart", roleUserId));

                        context.Set<Droit>().Add(Droit.New(userUiTest1.Entity.Id, "HeadStart", roleUserId));

                        context.Set<Droit>().Add(Droit.New(adminApiTest1.Entity.Id, "HeadStart", roleAdminId));
                        context.Set<Droit>().Add(Droit.New(adminApiTest2.Entity.Id, "HeadStart", roleAdminId));
                        context.Set<Droit>().Add(Droit.New(adminApiTest3.Entity.Id, "HeadStart", roleAdminId));

                        context.Set<Droit>().Add(Droit.New(adminUiTest1.Entity.Id, "HeadStart", roleAdminId));

                        context.Set<Droit>().Add(Droit.New(superAdmin.Entity.Id, "HeadStart", roleAdminId));

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
