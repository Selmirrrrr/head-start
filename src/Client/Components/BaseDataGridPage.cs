using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.WebUtilities;
using MudBlazor;

namespace HeadStart.Client.Components;

/// <summary>
/// Base class for pages that use MudDataGrid with URL state synchronization
/// Provides common functionality for filtering, sorting, and pagination
/// </summary>
/// <typeparam name="TItem">The type of item displayed in the grid</typeparam>
public abstract class BaseDataGridPage<TItem> : ComponentBase
{
    [Inject]
    public required NavigationManager NavigationManager { get; set; }

    protected MudDataGrid<TItem>? DataGrid;
    protected bool Loading;
    protected string? SearchString;

    /// <summary>
    /// Loads state from URL query parameters
    /// Override this to add custom query parameter handling
    /// </summary>
    protected virtual void LoadStateFromUrl()
    {
        var uri = new Uri(NavigationManager.Uri);
        var queryParams = QueryHelpers.ParseQuery(uri.Query);

        if (queryParams.TryGetValue("search", out var search))
        {
            SearchString = search.ToString();
        }
    }

    /// <summary>
    /// Updates the URL with current filter state
    /// Override this to add custom query parameters
    /// </summary>
    protected virtual void UpdateUrl(Dictionary<string, string?> additionalParams)
    {
        var queryParams = new Dictionary<string, string?>();

        if (!string.IsNullOrWhiteSpace(SearchString))
        {
            queryParams["search"] = SearchString;
        }

        // Merge additional parameters
        foreach (var param in additionalParams)
        {
            queryParams[param.Key] = param.Value;
        }

        var url = NavigationManager.GetUriWithQueryParameters(queryParams);
        NavigationManager.NavigateTo(url, replace: true);
    }

    /// <summary>
    /// Refreshes the data grid
    /// </summary>
    protected async Task RefreshData()
    {
        if (DataGrid != null)
        {
            await DataGrid.ReloadServerData();
        }
    }

    /// <summary>
    /// Builds a Gridify filter string from individual filter components
    /// </summary>
    protected static string? BuildGridifyFilter(params (bool condition, string filter)[] filters)
    {
        var activeFilters = filters
            .Where(f => f.condition)
            .Select(f => f.filter)
            .ToList();

        return activeFilters.Any() ? string.Join(",", activeFilters) : null;
    }

    /// <summary>
    /// Builds a Gridify sort string from MudBlazor sort definitions
    /// </summary>
    protected static string? BuildGridifySort<T>(ICollection<SortDefinition<T>> sortDefinitions)
    {
        if (!sortDefinitions.Any())
            return null;

        var sortExpressions = sortDefinitions
            .Select(s => $"{s.SortBy} {(s.Descending ? "desc" : "asc")}");

        return string.Join(", ", sortExpressions);
    }
}
