using HeadStart.Client.Extensions;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Serilog;
using Serilog.Exceptions;

namespace HeadStart.Client;

public static class Program
{
    public static async Task Main(string[] args)
    {
        try
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);

            var loggerConfig = new LoggerConfiguration()
                .MinimumLevel.Information()
                .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Information)
                .Enrich.FromLogContext()
                .Enrich.WithExceptionDetails()
                .Enrich.WithProperty("Application", "HeadStart.Client")
                .Enrich.WithProperty("Platform", "WebAssembly")
                .Enrich.WithProperty("InstanceId", Guid.CreateVersion7())
                .WriteTo.BrowserConsole()
                .WriteTo.BrowserHttp();

            builder.Logging.AddSerilog(loggerConfig.CreateLogger());
            builder.RootComponents.Add<App>("#app");

            builder.Services.AddClientLayer(builder.HostEnvironment);

            builder.Services.TryAddMudBlazor(builder.Configuration);

            var app = builder.Build();
            await app.SetDefaultUICulture();
            await app.RunAsync();
        }
        catch (Exception ex)
        {
            if (Log.Logger.GetType().Name == "SilentLogger")
            {
                Log.Logger = new LoggerConfiguration()
                    .MinimumLevel.Debug()
                    .WriteTo.BrowserConsole()
                    .CreateLogger();
            }

            Log.Fatal(ex, "Host terminated unexpectedly");
        }
        finally
        {
            Log.Information("Shut down complete");
            await Log.CloseAndFlushAsync();
        }
    }
}
