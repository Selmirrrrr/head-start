using HeadStart.Client.Generated;
using HeadStart.Client.Generated.Models;
using MudBlazor;

namespace HeadStart.Client.Components.Autocompletes;

public class MultiTenantAutocomplete: MudAutocomplete<HeadStartWebAPIFeaturesTenantsTenantsGetList_TenantViewModel>
{
    private readonly ApiClientV1 _apiClient;
    private static IList<HeadStartWebAPIFeaturesTenantsTenantsGetList_TenantViewModel> _tenants = [];

    public MultiTenantAutocomplete(ApiClientV1 apiClient)
    {
        _apiClient = apiClient;
        SearchFunc = SearchKeyValues;
        ToStringFunc = dto => dto?.Name;
        Dense = true;
        ResetValueOnEmptyText = true;
        ShowProgressIndicator = true;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender || !_tenants.Any())
        {
            _tenants = (await _apiClient.Api.V1.Tenants.GetAsync())?.Tenants ?? [];
        }
    }

    private Task<IEnumerable<HeadStartWebAPIFeaturesTenantsTenantsGetList_TenantViewModel>> SearchKeyValues(string? value, CancellationToken cancellation)
    {
        IEnumerable<HeadStartWebAPIFeaturesTenantsTenantsGetList_TenantViewModel> result;

        if (string.IsNullOrWhiteSpace(value))
            result = _tenants;
        else
            result = _tenants
                .Where(x => x.Name?.Contains(value, StringComparison.InvariantCultureIgnoreCase) == true ||
                            x.Name?.Contains(value, StringComparison.InvariantCultureIgnoreCase) == true)
                .ToList();

        return Task.FromResult(result);
    }
}
