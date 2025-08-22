using HeadStart.Aspire.ServiceDefaults;
using HeadStart.SharedKernel.Extensions;
using HeadStart.WebAPI.Extensions;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Configure Serilog
builder.Host.UseSerilog((builderContext, loggerConfig) =>
    loggerConfig.ConfigureWebApplicationLogging(
        builderContext.Configuration,
        builderContext.HostingEnvironment,
        "HeadStart.WebAPI"));

builder.Services.AddApiFramework();
builder.Services.AddDatabaseServices(builder.Configuration);
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
    app.ConfigureApiDocumentation();

    await app.InitializeDatabaseAsync();
    await app.InitializeKiotaAsync();

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
