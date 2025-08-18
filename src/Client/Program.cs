using HeadStart.Client.Extensions;
using HeadStart.SharedKernel.Extensions;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor;
using MudBlazor.Services;
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
                .Enrich.WithInstanceId()
                .Enrich.WithExceptionDetails()
                .WriteTo.BrowserConsole()
                .WriteTo.BrowserHttp();

            builder.Logging.AddSerilog(loggerConfig.CreateLogger());
            builder.RootComponents.Add<App>("#app");
            // builder.RootComponents.Add<HeadOutlet>("head::after");

            builder.Services.AddClientLayer(builder.HostEnvironment);

            builder.Services.AddMudServices(options =>
            {
                options.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.BottomRight;
            });

            await builder.Build().RunAsync();
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

