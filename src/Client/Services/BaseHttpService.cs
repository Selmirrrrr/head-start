using System.Net.Http.Json;
using System.Net.Mime;
using System.Text.Json;
using Ardalis.GuardClauses;
using HeadStart.SharedKernel;

namespace HeadStart.Client.Services;

using static Constants;

public abstract class BaseHttpService
{
    protected readonly HttpClient HttpClient;
    protected readonly JsonSerializerOptions JsonSerializerOptions = JsonSerializerConfigurations.Default;

    protected BaseHttpService(IHttpClientFactory httpClientFactory, bool authorized = false)
    {
        Guard.Against.Null(httpClientFactory);

        HttpClient = httpClientFactory.CreateClient(authorized ? Http.AuthorizedClientId : Http.UnauthorizedClientId);
    }

    protected async Task DeleteAsync(string uri, CancellationToken ct = default)
    {
        var response = await HttpClient.DeleteAsync(uri, ct);
        response.EnsureSuccessStatusCode();
    }

    protected Task<TRes?> GetAsync<TRes>(string uri, CancellationToken ct = default) =>
        HttpClient.GetFromJsonAsync<TRes>(uri, JsonSerializerOptions, ct);

    protected async Task PostAsync<TPayload>(string uri, TPayload payload, CancellationToken ct = default)
    {
        using var response = await HttpClient.PostAsJsonAsync(uri, payload, ct);
        response.EnsureSuccessStatusCode();
    }

    protected async Task PostAsync(string uri, HttpContent? content = null, CancellationToken ct = default)
    {
        using var response = await HttpClient.PostAsync(uri, content, ct);
        response.EnsureSuccessStatusCode();
    }

    protected async Task<TRes> PostAsync<TPayload, TRes>(string uri, TPayload payload, CancellationToken ct = default)
    {
        using var response = await HttpClient.PostAsJsonAsync(uri, payload, ct);
        response.EnsureSuccessStatusCode();

        return await ParseHttpResponseContentAsync<TRes>(response, ct);
    }

    protected async Task PutAsync<TPayload>(string uri, TPayload payload, CancellationToken ct = default)
    {
        using var response = await HttpClient.PutAsJsonAsync(uri, payload, ct);
        response.EnsureSuccessStatusCode();
    }

    protected async Task<TRes?> PutAsync<TPayload, TRes>(string uri, TPayload payload, CancellationToken ct = default)
    {
        using var response = await HttpClient.PutAsJsonAsync(uri, payload, ct);
        response.EnsureSuccessStatusCode();

        return await ParseHttpResponseContentAsync<TRes>(response, ct);
    }

    private async Task<TModel?> ParseHttpResponseContentAsync<TModel>(HttpResponseMessage response, CancellationToken ct = default)
    {
        TModel? model = default;
        if (response.Content.Headers.ContentType?.MediaType == MediaTypeNames.Application.Json)
        {
            model = await response.Content.ReadFromJsonAsync<TModel>(JsonSerializerOptions, ct);
        }

        return model;
    }
}

/// <inheritdoc cref="ITableService"/>
public class TableService : BaseHttpService, ITableService
{
    private const string Route = "/api/tables";
    private const string UserRoute = "/api/users/me";

    public TableService(IHttpClientFactory httpClientFactory)
        : base(httpClientFactory, true)
    {
    }

    public Task<Guid> GetByIdAsync(Guid id, CancellationToken ct = default)
        => GetAsync<Guid>($"{Route}/{id}", ct);

    public Task<Response> GetMe(CancellationToken ct = default)
        => GetAsync<Response>(UserRoute, ct)!;
}

public interface ITableService
{
    Task<Guid> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Response> GetMe(CancellationToken ct = default);
}

public record ClaimsViewModel(string Type, string Value);

public record Response(IList<ClaimsViewModel> Claims);
