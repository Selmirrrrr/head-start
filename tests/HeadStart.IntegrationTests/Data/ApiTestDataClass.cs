using System.Net.Http.Json;
using System.Text.Json;
using Ardalis.GuardClauses;
using HeadStart.Client.Generated;
using HeadStart.IntegrationTests.Helpers;
using Microsoft.Kiota.Abstractions.Authentication;
using Microsoft.Kiota.Http.HttpClientLibrary;
using TUnit.Core.Interfaces;

namespace HeadStart.IntegrationTests.Data;

/// <summary>
/// Test fixture that provides authenticated and unauthenticated API clients
/// for testing directly against the WebAPI using a test user token.
/// </summary>
public sealed class ApiTestDataClass : IAsyncInitializer, IAsyncDisposable
{
    private HttpClient? _authenticatedHttpClient;

    public ApiClientV1 Admin1ApiClient { get; private set; } = null!;
    public ApiClientV1 PlatformAdmin1ApiClient { get; private set; } = null!;
    public ApiClientV1 Admin2ApiClient { get; private set; } = null!;
    public ApiClientV1 Admin3ApiClient { get; private set; } = null!;

    public ApiClientV1 User1ApiClient { get; private set; } = null!;
    public ApiClientV1 User2ApiClient { get; private set; } = null!;
    public ApiClientV1 User3ApiClient { get; private set; } = null!;

    public ApiClientV1 AnonymousApiClient { get; private set; } = null!;

    public Uri BffUrl { get; private set; } = null!;
    public Uri WebApiUrl { get; private set; } = null!;
    public Uri KeycloakUrl { get; private set; } = null!;

    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
    };

    public async Task InitializeAsync()
    {
        Guard.Against.Null(GlobalSetup.App);

        // Get service URLs
        BffUrl = GlobalSetup.App.GetEndpoint("bff");
        WebApiUrl = GlobalSetup.App.GetEndpoint("webapi", endpointName: "https");
        KeycloakUrl = GlobalSetup.App.GetEndpoint("keycloak");

        // Wait for services to be ready
        if (GlobalSetup.NotificationService != null)
        {
            await Task.WhenAll(
                GlobalSetup.NotificationService.WaitForResourceAsync("webapi", KnownResourceStates.Running).WaitAsync(TimeSpan.FromSeconds(30)),
                GlobalSetup.NotificationService.WaitForResourceAsync("keycloak", KnownResourceStates.Running).WaitAsync(TimeSpan.FromSeconds(30))
            );
        }

        // Create authenticated API client that goes directly to WebAPI
        PlatformAdmin1ApiClient = await SetupApiClientAsync(Users.PlatformAdmin1.UserEmail, Users.PlatformAdmin1.UserPassword);
        Admin1ApiClient = await SetupApiClientAsync(Users.AdminApiTest1.UserEmail, Users.AdminApiTest1.UserPassword);
        Admin2ApiClient = await SetupApiClientAsync(Users.AdminApiTest2.UserEmail, Users.AdminApiTest2.UserPassword);
        Admin3ApiClient = await SetupApiClientAsync(Users.AdminApiTest3.UserEmail, Users.AdminApiTest3.UserPassword);
        User1ApiClient = await SetupApiClientAsync(Users.UserApiTest1.UserEmail, Users.UserApiTest1.UserPassword);
        User2ApiClient = await SetupApiClientAsync(Users.UserApiTest2.UserEmail, Users.UserApiTest2.UserPassword);
        User3ApiClient = await SetupApiClientAsync(Users.UserApiTest3.UserEmail, Users.UserApiTest3.UserPassword);
        AnonymousApiClient = await SetupApiClientAsync(null, null);
    }

    private async Task<ApiClientV1> SetupApiClientAsync(string? username, string? password)
    {
        _authenticatedHttpClient = new HttpClient
        {
            BaseAddress = WebApiUrl
        };

        IAuthenticationProvider authProvider;
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            authProvider = new AnonymousAuthenticationProvider();
        }
        else
        {
            authProvider = new HeadStartAuthenticationProvider((await GetTestUserTokenAsync(username, password)).AccessToken);
        }

        var requestAdapter = new HttpClientRequestAdapter(authProvider, httpClient: _authenticatedHttpClient);

        return new ApiClientV1(requestAdapter);
    }

    private async Task<TokenResponse> GetTestUserTokenAsync(string username = "user1@example.com", string password = "user1")
    {
        using var keycloakClient = new HttpClient();
        var tokenEndpoint = $"{KeycloakUrl}/realms/HeadStart/protocol/openid-connect/token";

        var tokenRequest = new FormUrlEncodedContent([
            new KeyValuePair<string, string>("grant_type", "password"),
            new KeyValuePair<string, string>("client_id", "HeadStart-Test"),
            new KeyValuePair<string, string>("client_secret", "eaH1n5YflVUjTlCLQVPVyInPF4Z41VVb"),
            new KeyValuePair<string, string>("username", username),
            new KeyValuePair<string, string>("password", password),
            new KeyValuePair<string, string>("scope", "openid profile"),
            new KeyValuePair<string, string>("audience", "headstart.api")
        ]);

        var tokenResponse = await keycloakClient.PostAsync(tokenEndpoint, tokenRequest);

        if (!tokenResponse.IsSuccessStatusCode)
        {
            var errorContent = await tokenResponse.Content.ReadAsStringAsync();
            throw new InvalidOperationException($"Failed to get token: {tokenResponse.StatusCode} - {errorContent}");
        }

        var tokenData = await tokenResponse.Content.ReadFromJsonAsync<TokenResponse>(_jsonOptions)
            ?? throw new InvalidOperationException("Failed to deserialize token response");

        return tokenData;
    }

    public async ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);
        _authenticatedHttpClient?.Dispose();
        await Task.CompletedTask;
    }

    private class TokenResponse
    {
        public string AccessToken { get; init; } = string.Empty;
        public string RefreshToken { get; init; } = string.Empty;
        public string TokenType { get; init; } = string.Empty;
    }
}

public sealed class HeadStartAuthenticationProvider(string apiKey) : ApiKeyAuthenticationProvider("Bearer " + apiKey, "Authorization", KeyLocation.Header);
