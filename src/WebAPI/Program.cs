using CorrelationId;
using FastEndpoints;
using FastEndpoints.Swagger;
using HeadStart.Aspire.ServiceDefaults;
using HeadStart.SharedKernel.Extensions;
using HeadStart.WebAPI.Extensions;
using Scalar.AspNetCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Configure Serilog
builder.Host.UseSerilog((builderContext, loggerConfig)
    => loggerConfig.ConfigureFromSettings(builderContext.Configuration));

builder.Services.AddFastEndpoints().AddSwaggerDocument();
// Add services
builder.Services.AddSharedKernelServices();
builder.Services.AddApiServices(builder.Configuration);

var app = builder.Build();

app.MapDefaultEndpoints();
app.UseFastEndpoints()
    .UseSwaggerGen()
    .UseOpenApi();

try
{
    // Configure middleware
    app.UseResponseCompression();
    if (app.Environment.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
        //scalar by default looks for the swagger json file here:
        app.UseOpenApi(c => c.Path = "/openapi/{documentName}.json");
        app.MapScalarApiReference();
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
