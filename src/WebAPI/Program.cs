using FastEndpoints.ClientGen.Kiota;
using Gridify;
using HeadStart.Aspire.ServiceDefaults;
using HeadStart.SharedKernel.Extensions;
using HeadStart.SharedKernel.Logging;
using HeadStart.WebAPI.Core.Extensions;
using Serilog;
using Serilog.Debugging;

GridifyGlobalConfiguration.EnableEntityFrameworkCompatibilityLayer();

var builder = WebApplication.CreateBuilder(args);

// Enable Serilog internal logging to troubleshoot issues
if (builder.Configuration["Serilog:SelfLog:Enabled"] == "true")
{
    SelfLog.Enable(msg =>
    {
        Console.WriteLine($"[Serilog SelfLog] {msg}");
        System.Diagnostics.Debug.WriteLine($"[Serilog SelfLog] {msg}");
    });
}

builder.AddServiceDefaults();

// Configure Serilog
builder.Host.UseSerilog((builderContext, loggerConfig) =>
    loggerConfig.ConfigureWebApplicationLogging(
        builderContext.Configuration,
        builderContext.HostingEnvironment,
        "HeadStart.WebAPI"));

builder.Services.AddApiFramework(builder.Configuration);
builder.AddDatabaseServices();
builder.Services.AddSecurityServices();
builder.Services.AddAuth(builder.Environment.IsDevelopment());

builder.Services.AddSharedKernelServices();

var app = builder.Build();

app.MapDefaultEndpoints();

try
{
    app.ConfigureExceptionHandling();
    app.ConfigureRequestProcessing();
    app.ConfigureAuthentication();

    // UserDataLoggingMiddleware wraps everything that follows with LogContext
    app.UseMiddleware<UserDataLoggingMiddleware>();

    // HTTP logging will now run within the enriched LogContext and inherit UserId
    app.UseHttpLogging();

    app.ConfigureApiDocumentation();

    if (app.IsNotGenerationMode())
    {
        await app.InitializeDatabaseAsync();
    }
    else if (app.IsApiClientGenerationMode() || app.IsSwaggerJsonExportMode())
    {
        await app.InitializeKiotaAsync();
    }

    await app.RunAsync();
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
    await Log.CloseAndFlushAsync();
}
