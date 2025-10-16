using EvolveDb;
using FastEndpoints;
using FastEndpoints.ClientGen.Kiota;
using FastEndpoints.Swagger;
using HeadStart.WebAPI.Core.Filters;
using HeadStart.WebAPI.Core.Processors;
using HeadStart.WebAPI.Data;
using Kiota.Builder;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Scalar.AspNetCore;
using Serilog;

namespace HeadStart.WebAPI.Core.Extensions;

internal static class WebApplicationExtensions
{
    internal static async Task InitializeKiotaAsync(this WebApplication app)
    {
        await app.GenerateApiClientsAndExitAsync(
            c =>
            {
                c.SwaggerDocumentName = "headstart-api-v1"; //must match doc name above
                c.Language = GenerationLanguage.CSharp;
                c.OutputPath = "../Client/Generated/"; //relative to the project root
                c.ClientNamespaceName = "HeadStart.Client.Generated";
                c.ClientClassName = "ApiClientV1";
                c.CleanOutput = true;
                c.Deserializers = ["Microsoft.Kiota.Serialization.Json.JsonParseNodeFactory"];
                c.Serializers = ["Microsoft.Kiota.Serialization.Json.JsonSerializationWriterFactory"];
            });
    }

    internal static async Task InitializeDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<HeadStartDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

        try
        {
            logger.LogInformation("Ensuring database is created...");

            // Use the execution strategy to handle retry logic properly
            var strategy = context.Database.CreateExecutionStrategy();
            await strategy.ExecuteAsync(async () =>
            {
                // Create database if it doesn't exist
                var created = await context.Database.EnsureCreatedAsync();
                if (created)
                {
                    logger.LogInformation("Database created successfully");
                }
                else
                {
                    logger.LogInformation("Database already exists");
                }

                // Apply any pending migrations
                var pendingMigrations = await context.Database.GetPendingMigrationsAsync();
                var migrations = pendingMigrations.ToList();
                if (migrations.Count != 0)
                {
                    logger.LogInformation("Applying {Count} pending migrations: {Migrations}",
                        migrations.Count, string.Join(", ", migrations));

                    await context.Database.MigrateAsync();
                    logger.LogInformation("Migrations applied successfully");
                }
                else
                {
                    logger.LogInformation("No pending migrations");
                }
            });

            var evolve = new Evolve(new NpgsqlConnection(app.Configuration.GetConnectionString("postgresdb")), msg => logger.LogInformation(msg))
            {
                MetadataTableSchema = "audit",
                Locations = ["Data/Scripts"],
                IsEraseDisabled = !app.Environment.IsDevelopment(),
            };

            evolve.Migrate();

            logger.LogInformation("Database initialization completed");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while ensuring database creation {App}", app.Environment.ApplicationName);
            throw new InvalidDataException("An error occurred while ensuring database creation", ex);
        }
    }

    internal static void ConfigureExceptionHandling(this WebApplication app)
    {
        app.UseResponseCompression();
        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
        else
        {
            app.UseExceptionHandler("/Error");
            app.UseHsts();
        }
    }

    internal static void ConfigureRequestProcessing(this WebApplication app)
    {
        app.UseHttpsRedirection();
        app.UseSerilogRequestLogging();
        app.UseRouting();
    }

    internal static void ConfigureAuthentication(this WebApplication app)
    {
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseStatusCodePages();
    }

    internal static void ConfigureApiDocumentation(this WebApplication app)
    {
        app.UseFastEndpoints(c =>
            {
                c.Errors.UseProblemDetails();
                c.Endpoints.RoutePrefix = "api";
                c.Versioning.Prefix = "v";
                c.Versioning.DefaultVersion = 1;
                c.Versioning.PrependToRoute = true;
                c.Endpoints.Configurator = ep =>
                {
                    ep.PostProcessor<AuditRequestProcessor>(Order.After);
                    ep.Options(b => b.AddEndpointFilter<OperationCancelledFilter>());
                };
            })

           .UseSwaggerGen();

        if (app.Environment.IsDevelopment())
        {
            app.MapScalarApiReference(options =>
            {
                options
                    .WithTitle("HeadStart API")
                    .WithOpenApiRoutePattern("/swagger/{documentName}/swagger.json")
                    .AddPreferredSecuritySchemes("OAuth2")
                    .AddPasswordFlow("OAuth2", flow =>
                    {
                        flow.ClientId = "HeadStart-Test";
                        flow.Username = "user1@example.com";
                        flow.Password = "user1";
                        flow.ClientSecret = "eaH1n5YflVUjTlCLQVPVyInPF4Z41VVb";
                    });
            });
        }
    }
}
