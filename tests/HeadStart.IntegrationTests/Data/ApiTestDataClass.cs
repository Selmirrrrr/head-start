using System.Net.Http.Json;
using System.Text.Json;
using Ardalis.GuardClauses;
using HeadStart.Client.Generated;
using Microsoft.Kiota.Abstractions.Authentication;
using Microsoft.Kiota.Http.HttpClientLibrary;
using TUnit.Core.Interfaces;

namespace HeadStart.IntegrationTests.Data;

/// <summary>
/// Test fixture that provides authenticated and unauthenticated API clients
/// for testing directly against the WebAPI using a test user token.
/// </summary>
public class ApiTestDataClass() : IAsyncInitializer, IAsyncDisposable
{
    private HttpClient? _authenticatedHttpClient;

    public ApiClientV1 ApiClient { get; private set; } = null!;
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
        await SetupAuthenticatedApiClientAsync();
    }

    private async Task SetupAuthenticatedApiClientAsync()
    {
        // Get a test user token from Keycloak using password grant
        var tokenData = await GetTestUserTokenAsync();

        _authenticatedHttpClient = new HttpClient()
        {
            BaseAddress = WebApiUrl
        };

        var authProvider = new HeadStartAuthenticationProvider(tokenData.AccessToken);
        var requestAdapter = new HttpClientRequestAdapter(authProvider, httpClient: _authenticatedHttpClient);

        ApiClient = new ApiClientV1(requestAdapter);
    }

    private async Task<TokenResponse> GetTestUserTokenAsync()
    {
        using var keycloakClient = new HttpClient();
        var tokenEndpoint = $"{KeycloakUrl}/realms/HeadStart/protocol/openid-connect/token";

        var tokenRequest = new FormUrlEncodedContent([
            new KeyValuePair<string, string>("grant_type", "password"),
            new KeyValuePair<string, string>("client_id", "HeadStart-Test"),
            new KeyValuePair<string, string>("client_secret", "eaH1n5YflVUjTlCLQVPVyInPF4Z41VVb"),
            new KeyValuePair<string, string>("username", "user"),
            new KeyValuePair<string, string>("password", "user"),
            new KeyValuePair<string, string>("scope", "openid profile"),
            new KeyValuePair<string, string>("audience", "headstart.api") // Required for WebAPI access
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

    public virtual async ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);
        _authenticatedHttpClient?.Dispose();
        await Task.CompletedTask;
    }

    private class TokenResponse
    {
        public string AccessToken { get; init; } = string.Empty;
        public string RefreshToken { get; init; } = string.Empty;
        public int ExpiresIn { get; init; }
        public string TokenType { get; init; } = string.Empty;
    }
}

public sealed class HeadStartAuthenticationProvider : ApiKeyAuthenticationProvider
{
    public HeadStartAuthenticationProvider(string apiKey)
        : base("Bearer " + apiKey, "Authorization", KeyLocation.Header)
    {
    }
}

