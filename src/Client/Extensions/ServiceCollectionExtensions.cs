using System.Net.Http.Headers;
using System.Net.Mime;
using Ardalis.GuardClauses;
using Blazored.LocalStorage;
using HeadStart.Client.Authorization;
using HeadStart.Client.Services;
using HeadStart.SharedKernel;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Radzen;

namespace HeadStart.Client.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers dependencies for the Blazor Client Application.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="environment"></param>
    /// <returns>An instance of <see cref="IServiceCollection"/>.</returns>
    public static void AddClientLayer(this IServiceCollection services, IWebAssemblyHostEnvironment environment)
    {
        Guard.Against.Null(services);
        Guard.Against.Null(environment);

        services.AddOptions();
        services.AddAuthorizationCore();
        services.AddScoped<DialogService>();
        services.AddScoped<NotificationService>();
        services.AddScoped<TooltipService>();
        services.AddScoped<ContextMenuService>();
        services.AddScoped<ITableService, TableService>();
        services.TryAddSingleton<AuthenticationStateProvider, HostAuthenticationStateProvider>();
        services.TryAddSingleton(sp => (HostAuthenticationStateProvider)sp.GetRequiredService<AuthenticationStateProvider>());
        services.AddTransient<AuthorizedHandler>();

        services.AddBlazoredLocalStorage(config =>
        {
            config.JsonSerializerOptions = JsonSerializerConfigurations.LocalStorageSettings;
        });

        services.AddHttpClient(Constants.Http.UnauthorizedClientId, client =>
        {
            client.BaseAddress = new Uri(environment.BaseAddress);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(MediaTypeNames.Application.Json));
        });

        services.AddHttpClient(Constants.Http.AuthorizedClientId, client =>
            {
                client.BaseAddress = new Uri(environment.BaseAddress);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(MediaTypeNames.Application.Json));
            })
            .AddHttpMessageHandler<AuthorizedHandler>();

        services.AddTransient(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("default"));

        services.AddSingleton(JsonSerializerConfigurations.Default);
        services.AddKeyedSingleton(nameof(JsonSerializerConfigurations.LoggingSettings), JsonSerializerConfigurations.LoggingSettings);

        services.AddRadzenComponents();
    }
}
