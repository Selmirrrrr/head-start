using HeadStart.Aspire.ServiceDefaults;
using HeadStart.BFF.Extensions;
using HeadStart.SharedKernel.Extensions;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Configure Serilog
builder.Host.UseSerilog((builderContext, loggerConfig) =>
    loggerConfig.ConfigureWebApplicationLogging(
        builder,
        "HeadStart.BFF"));

// Add services
builder.Services.AddSharedKernelServices();
builder.Services.AddBffServices(builder.Configuration, builder.Environment.IsDevelopment());

var app = builder.Build();

app.MapDefaultEndpoints();

try
{
    app.ConfigureBffPipeline();
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

