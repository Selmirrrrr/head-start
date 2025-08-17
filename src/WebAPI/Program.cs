using CorrelationId;
using FastEndpoints;
using FastEndpoints.ClientGen.Kiota;
using FastEndpoints.Swagger;
using HeadStart.Aspire.ServiceDefaults;
using HeadStart.SharedKernel.Extensions;
using HeadStart.WebAPI.Extensions;
using Kiota.Builder;
using Microsoft.AspNetCore.Http.Features;
using Scalar.AspNetCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Configure Serilog
builder.Host.UseSerilog((builderContext, loggerConfig)
    => loggerConfig.ConfigureFromSettings(builderContext.Configuration));

builder.Services.AddFastEndpoints()
    .SwaggerDocument(o =>
    {
        o.DocumentSettings = s =>
        {
            s.Title = "HeadStart API";
            s.Version = "v1";
            s.DocumentName = "HeadStartAPIv1";
        };
    });
// Add services
builder.Services.AddSharedKernelServices();
builder.Services.AddApiServices(builder.Configuration);

builder.Services.AddDataProtection(o => o.ApplicationDiscriminator = "HeadStart");

builder.Services.AddProblemDetails(options =>
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

var app = builder.Build();

app.MapDefaultEndpoints();

try
{
    // Configure middleware
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

    app.UseHttpsRedirection();
    app.UseCorrelationId();
    app.UseSerilogIngestion();
    app.UseSerilogRequestLogging();
    app.UseRouting();
    app.UseAuthentication();
    app.UseAuthorization();
    app.UseStatusCodePages();
    // Configure FastEndpoints and OpenAPI
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

    await app.GenerateApiClientsAndExitAsync(
        c =>
        {
            c.SwaggerDocumentName = "HeadStartAPIv1"; //must match doc name above
            c.Language = GenerationLanguage.CSharp;
            c.OutputPath = "../Client/Generated"; //relative to the project root
            c.ClientNamespaceName = "HeadStart.Client.Generated";
            c.ClientClassName = "ApiClient";
            c.CleanOutput = true;
            c.Deserializers = ["Microsoft.Kiota.Serialization.Json.JsonParseNodeFactory"];
            c.Serializers = ["Microsoft.Kiota.Serialization.Json.JsonSerializationWriterFactory"];
        });

    app.Run();
}
catch (Exception ex)
{
    if (Log.Logger.GetType().Name == "SilentLogger")
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Console()
            .CreateLogger();
    }

    Log.Fatal(ex, "Host terminated unexpectedly");
}
finally
{
    Log.Information("Shut down complete");
    Log.CloseAndFlush();
}
