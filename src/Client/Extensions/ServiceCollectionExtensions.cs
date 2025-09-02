using System.Net.Http.Headers;
using System.Net.Mime;
using Ardalis.GuardClauses;
using Blazored.LocalStorage;
using HeadStart.Client.Authorization;
using HeadStart.Client.Configurations;
using HeadStart.Client.Generated;
using HeadStart.Client.Services;
using HeadStart.Client.Services.Navigation;
using HeadStart.Client.Services.Notifications;
using HeadStart.Client.Services.Users;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Kiota.Abstractions;
using Microsoft.Kiota.Abstractions.Authentication;
using Microsoft.Kiota.Http.HttpClientLibrary;
using Microsoft.Kiota.Serialization.Json;
using MudBlazor;
using MudBlazor.Services;

namespace HeadStart.Client.Extensions;

public static class ServiceCollectionExtensions
{
    public static void TryAddMudBlazor(this IServiceCollection services, IConfiguration config)
    {
        services.AddMudServices(config =>
        {
            MudGlobal.InputDefaults.ShrinkLabel = true;
            config.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.BottomCenter;
            config.SnackbarConfiguration.NewestOnTop = false;
            config.SnackbarConfiguration.ShowCloseIcon = true;
            config.SnackbarConfiguration.VisibleStateDuration = 3000;
            config.SnackbarConfiguration.HideTransitionDuration = 500;
            config.SnackbarConfiguration.ShowTransitionDuration = 500;
            config.SnackbarConfiguration.SnackbarVariant = Variant.Filled;
        });
        services.AddLocalization(options => options.ResourcesPath = "Resources");
        services.AddMudPopoverService();
        services.AddMudBlazorSnackbar();
        services.AddMudBlazorDialog();
        services.AddMudLocalization();
        services.AddBlazoredLocalStorage();
        services.AddScoped<IStorageService, LocalStorageService>();
        services.AddScoped<DialogServiceHelper>();
    }
    /// <summary>
    /// Registers dependencies for the Blazor Client Application.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="environment"></param>
    public static void AddClientLayer(this IServiceCollection services, IWebAssemblyHostEnvironment environment)
    {
        Guard.Against.Null(services);
        Guard.Against.Null(environment);

        services.AddOptions();
        services.AddAuthorizationCore();
        services.AddLocalization();
        services.AddScoped<DialogService>();
        services.TryAddSingleton<AuthenticationStateProvider, HostAuthenticationStateProvider>();
        services.TryAddSingleton(sp => (HostAuthenticationStateProvider)sp.GetRequiredService<AuthenticationStateProvider>());
        services.AddTransient<AuthorizedHandler>();
        services.AddScoped<IMenuService, MenuService>();
        services.AddScoped<InMemoryNotificationService>();
        services.AddSingleton<UserStateService>();
        services.AddScoped<UserStateContainer>();
        services.AddScoped<INotificationService>(sp =>
        {
            var service = sp.GetRequiredService<InMemoryNotificationService>();
            service.Preload();
            return service;
        });

        // Configuration
        services.AddSingleton(new ClientAppSettings()!);

        services.AddBlazoredLocalStorage(config => config.JsonSerializerOptions = JsonSerializerConfigurations.LocalStorageSettings);

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

        services.AddScoped<ApiClientV1>(sp =>
        {
            ApiClientBuilder.RegisterDefaultSerializer<JsonSerializationWriterFactory>();
            ApiClientBuilder.RegisterDefaultDeserializer<JsonParseNodeFactory>();

            var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
            // Use the authorized HTTP client that includes the AuthorizedHandler
            var httpClient = httpClientFactory.CreateClient(Constants.Http.AuthorizedClientId);
            var authProvider = new AnonymousAuthenticationProvider();
            var requestAdapter = new HttpClientRequestAdapter(authProvider, httpClient: httpClient);
            var apiClient = new ApiClientV1(requestAdapter);

            requestAdapter.BaseUrl = environment.BaseAddress;
            return apiClient;
        });

        services.AddTransient(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("default"));

        services.AddSingleton(JsonSerializerConfigurations.Default);
        services.AddKeyedSingleton(nameof(JsonSerializerConfigurations.LoggingSettings), JsonSerializerConfigurations.LoggingSettings);
    }
}
