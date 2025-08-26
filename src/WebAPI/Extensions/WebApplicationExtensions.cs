using CorrelationId;
using FastEndpoints;
using FastEndpoints.ClientGen.Kiota;
using FastEndpoints.Swagger;
using HeadStart.WebAPI.Data;
using Kiota.Builder;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using Serilog;

namespace HeadStart.WebAPI.Extensions;

internal static class WebApplicationExtensions
{
    internal static async Task InitializeKiotaAsync(this WebApplication app)
    {
        await app.GenerateApiClientsAndExitAsync(
            c =>
            {
                c.SwaggerDocumentName = "headstart-api-v1"; //must match doc name above
                c.Language = GenerationLanguage.CSharp;
                c.OutputPath = "../Client/Generated"; //relative to the project root
                c.ClientNamespaceName = "HeadStart.Client.Generated";
                c.ClientClassName = "ApiClient";
                c.CleanOutput = true;
                c.Deserializers = ["Microsoft.Kiota.Serialization.Json.JsonParseNodeFactory"];
                c.Serializers = ["Microsoft.Kiota.Serialization.Json.JsonSerializationWriterFactory"];
            });

        await app.ExportSwaggerJsonAndExitAsync("headstart-api-v1", "./docs/");
    }

    internal static async Task InitializeDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<HeadStartDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

        try
        {
            logger.LogInformation("Ensuring database is created...");

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

            // Seed data (this will run automatically due to UseAsyncSeeding configuration)
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
        app.UseCorrelationId();
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
        app.UseFastEndpoints()
           .UseSwaggerGen();

        if (app.Environment.IsDevelopment())
        {
            app.MapScalarApiReference(options =>
            {
                options
                    .WithTitle("HeadStart API")
                    .WithOpenApiRoutePattern("/swagger/{documentName}/swagger.json");
            });
        }
    }
}
