using System.Globalization;
using Blazored.LocalStorage;
using HeadStart.Client.Extensions;
using HeadStart.Client.Services;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.JSInterop;
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

            var host = builder.Build();

            // Initialize culture
            var storageService = host.Services.GetRequiredService<ILocalStorageService>();
            
            // Try to get saved culture from local storage
            string? culture = null;
            try
            {
                culture = await storageService.GetItemAsStringAsync("_Culture");
            }
            catch
            {
                // Ignore errors when reading from storage
            }

            // If no saved culture, try to get from query string
            if (string.IsNullOrEmpty(culture))
            {
                var uri = new Uri(builder.HostEnvironment.BaseAddress);
                var queryParams = System.Web.HttpUtility.ParseQueryString(uri.Query);
                culture = queryParams["culture"];
            }

            // If still no culture, use French as default
            if (string.IsNullOrEmpty(culture))
            {
                culture = "fr";
            }

            // Remove quotes if present from localStorage
            culture = culture?.Trim('"') ?? "fr";

            // Set the culture with fallback
            try
            {
                var cultureInfo = new CultureInfo(culture);
                CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
                CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;
            }
            catch (CultureNotFoundException)
            {
                // If the specific culture is not found, try the neutral culture
                try
                {
                    var neutralCulture = culture.Contains("-") ? culture.Split('-')[0] : culture;
                    var cultureInfo = new CultureInfo(neutralCulture);
                    CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
                    CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;
                }
                catch
                {
                    // Fall back to invariant culture if all else fails
                    var cultureInfo = CultureInfo.InvariantCulture;
                    CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
                    CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;
                }
            }

            await host.RunAsync();
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

