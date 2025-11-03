# DataGrid Implementation Template

This template shows how to create a new DataGrid endpoint and page following the pattern established in AuditTrailsGetList.

## Backend (WebAPI) Implementation

### 1. Create Endpoint File: `src/WebAPI/Features/[YourFeature]/[Entity]GetList.cs`

```csharp
using FastEndpoints;
using Gridify;
using HeadStart.SharedKernel.Models.Models;
using HeadStart.WebAPI.Data;
using HeadStart.WebAPI.Data.Models;
using HeadStart.WebAPI.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace HeadStart.WebAPI.Features.[YourFeature];

public static class [Entity]GetList
{
    public class Endpoint : Endpoint<Request, GridifyPagedResponse<[Entity]Dto>>
    {
        public required HeadStartDbContext DbContext { get; set; }

        public override void Configure()
        {
            Get("/[your-feature]/[entities]");
            Version(1);
            // Add authorization if needed
            // Policies(PolicyNames.YourPolicy);
        }

        public override async Task HandleAsync(Request req, CancellationToken ct)
        {
            // Build the query - include related entities if needed
            var query = DbContext.[Entities]
                .Include(e => e.RelatedEntity) // Optional: include related entities
                .AsNoTracking();

            // Build Gridify query from request parameters
            var gridifyQuery = new GridifyQuery
            {
                Page = req.Page ?? 1,
                PageSize = req.PageSize ?? 10,
                Filter = req.Filter,
                OrderBy = req.OrderBy ?? "[DefaultSortProperty] desc"
            };

            // Apply Gridify filtering, sorting, and pagination
            var result = await query.GridifyQueryableAsync(gridifyQuery, ct);

            // Map to DTOs and create response
            var response = result.ToPagedResponse(
                MapToDto,
                gridifyQuery.Page,
                gridifyQuery.PageSize
            );

            await SendOkAsync(response, ct);
        }

        private static [Entity]Dto MapToDto([Entity] entity)
        {
            return new [Entity]Dto
            {
                // Map properties
                Id = entity.Id,
                Name = entity.Name,
                // ... other properties
            };
        }
    }

    public class Request
    {
        public int? Page { get; set; }
        public int? PageSize { get; set; }
        public string? Filter { get; set; }
        public string? OrderBy { get; set; }
    }

    public record [Entity]Dto
    {
        public required Guid Id { get; init; }
        public required string Name { get; init; }
        // ... other properties
    };
}
```

## Frontend (Blazor) Implementation

### 2. Create Page File: `src/Client/Pages/[YourFeature]/[Entities].razor`

```razor
@page "/[your-feature]/[entities]"
@using HeadStart.Client.Generated
@using HeadStart.Client.Generated.Models
@using Microsoft.AspNetCore.WebUtilities
@inject ApiClientV1 ApiClient
@inject IStringLocalizer<[Entities]> LC
@inject NavigationManager NavigationManager
@inject IDialogService DialogService

<PageTitle>@LC["[Entities]"]</PageTitle>

<MudText Typo="Typo.h4" Class="my-4">@LC["[Entities]"]</MudText>

<MudPaper Class="pa-4" Elevation="2">
    <MudDataGrid @ref="_dataGrid"
                 T="[GeneratedDtoType]"
                 ServerData="LoadServerData"
                 Filterable="true"
                 FilterMode="DataGridFilterMode.ColumnFilterRow"
                 SortMode="SortMode.Multiple"
                 Hideable="true"
                 ColumnResizeMode="ResizeMode.Column"
                 Dense="false"
                 Hover="true"
                 Loading="@_loading"
                 LoadingProgressColor="Color.Info"
                 FixedHeader="true"
                 Height="calc(100vh - 280px)">
        <ToolBarContent>
            <MudText Typo="Typo.h6">@LC["[Entity] Records"]</MudText>
            <MudSpacer />
            <MudTextField @bind-Value="_searchString"
                          Placeholder="@LC["Search"]"
                          Adornment="Adornment.Start"
                          AdornmentIcon="@Icons.Material.Filled.Search"
                          IconSize="Size.Medium"
                          Class="mt-0"
                          Immediate="true"
                          DebounceInterval="500"
                          OnDebounceIntervalElapsed="OnSearchChanged">
            </MudTextField>
            <MudIconButton Icon="@Icons.Material.Filled.Refresh"
                           OnClick="RefreshData"
                           Color="Color.Primary"
                           Class="ml-2">
            </MudIconButton>
        </ToolBarContent>
        <Columns>
            <!-- Add your columns here -->
            <PropertyColumn Property="x => x.Name"
                            Title="@LC["Name"]"
                            Sortable="true"
                            Filterable="true">
                <FilterTemplate>
                    <MudTextField @bind-Value="_nameFilter"
                                  Placeholder="@LC["Filter by name"]"
                                  Immediate="true"
                                  Class="mt-0" />
                </FilterTemplate>
            </PropertyColumn>

            <!-- Add more columns as needed -->

            <TemplateColumn Sortable="false" Filterable="false" CellClass="d-flex justify-end">
                <CellTemplate>
                    <MudIconButton Icon="@Icons.Material.Filled.RemoveRedEye"
                                   Size="Size.Small"
                                   Color="Color.Primary"
                                   OnClick="@(() => ShowDetails(context.Item))"
                                   Title="@LC["View Details"]">
                    </MudIconButton>
                </CellTemplate>
            </TemplateColumn>
        </Columns>
        <PagerContent>
            <MudDataGridPager T="[GeneratedDtoType]" />
        </PagerContent>
    </MudDataGrid>
</MudPaper>

@code {
    private MudDataGrid<[GeneratedDtoType]>? _dataGrid;
    private bool _loading;
    private string? _searchString;
    private string? _nameFilter; // Add filter fields as needed

    protected override void OnInitialized()
    {
        LoadStateFromUrl();
    }

    private void LoadStateFromUrl()
    {
        var uri = new Uri(NavigationManager.Uri);
        var queryParams = QueryHelpers.ParseQuery(uri.Query);

        if (queryParams.TryGetValue("search", out var search))
            _searchString = search.ToString();

        if (queryParams.TryGetValue("name", out var name))
            _nameFilter = name.ToString();

        // Add more parameter loading as needed
    }

    private void UpdateUrl()
    {
        var queryParams = new Dictionary<string, string?>();

        if (!string.IsNullOrWhiteSpace(_searchString))
            queryParams["search"] = _searchString;

        if (!string.IsNullOrWhiteSpace(_nameFilter))
            queryParams["name"] = _nameFilter;

        // Add more parameters as needed

        var url = NavigationManager.GetUriWithQueryParameters(queryParams);
        NavigationManager.NavigateTo(url, replace: true);
    }

    private async Task<GridData<[GeneratedDtoType]>> LoadServerData(GridState<[GeneratedDtoType]> state)
    {
        _loading = true;

        try
        {
            // Build Gridify filter expression
            var filters = new List<string>();

            if (!string.IsNullOrWhiteSpace(_searchString))
            {
                filters.Add($"(Name|Description)@=*{_searchString}"); // Adjust search fields
            }

            if (!string.IsNullOrWhiteSpace(_nameFilter))
            {
                filters.Add($"Name@=*{_nameFilter}");
            }

            // Add more filters as needed

            var filterString = filters.Any() ? string.Join(",", filters) : null;

            // Build sort expression
            string? orderBy = null;
            if (state.SortDefinitions.Any())
            {
                var sortExpressions = state.SortDefinitions.Select(s =>
                    $"{s.SortBy} {(s.Descending ? "desc" : "asc")}");
                orderBy = string.Join(", ", sortExpressions);
            }

            // Update URL
            UpdateUrl();

            // Call API
            var response = await ApiClient.Api.V1.[YourFeature].[Entities].GetAsync(requestConfiguration =>
            {
                requestConfiguration.QueryParameters.Page = state.Page + 1;
                requestConfiguration.QueryParameters.PageSize = state.PageSize;
                requestConfiguration.QueryParameters.Filter = filterString;
                requestConfiguration.QueryParameters.OrderBy = orderBy;
            });

            return new GridData<[GeneratedDtoType]>
            {
                Items = response?.Data ?? [],
                TotalItems = response?.TotalCount ?? 0
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading data: {ex.Message}");
            return new GridData<[GeneratedDtoType]>
            {
                Items = [],
                TotalItems = 0
            };
        }
        finally
        {
            _loading = false;
        }
    }

    private async Task OnSearchChanged()
    {
        if (_dataGrid != null)
        {
            await _dataGrid.ReloadServerData();
        }
    }

    private async Task RefreshData()
    {
        if (_dataGrid != null)
        {
            await _dataGrid.ReloadServerData();
        }
    }

    private async Task ShowDetails([GeneratedDtoType] item)
    {
        var parameters = new DialogParameters<[Entity]DetailDialog>
        {
            { x => x.[Entity], item }
        };

        var options = new DialogOptions
        {
            MaxWidth = MaxWidth.Large,
            FullWidth = true,
            CloseButton = true,
            DisableBackdropClick = false
        };

        await DialogService.ShowAsync<[Entity]DetailDialog>(LC["[Entity] Details"], parameters, options);
    }
}
```

### 3. Create Detail Dialog: `src/Client/Pages/[YourFeature]/[Entity]DetailDialog.razor`

```razor
@using HeadStart.Client.Generated.Models
@inject IStringLocalizer<[Entity]DetailDialog> LC

<MudDialog>
    <TitleContent>
        <MudText Typo="Typo.h6">
            <MudIcon Icon="@Icons.Material.Filled.Info" Class="mr-2" />
            @LC["[Entity] Details"]
        </MudText>
    </TitleContent>
    <DialogContent>
        @if ([Entity] != null)
        {
            <MudGrid>
                <MudItem xs="12">
                    <MudPaper Class="pa-4" Elevation="0" Outlined="true">
                        <MudText Typo="Typo.subtitle2" Color="Color.Primary" GutterBottom="true">
                            @LC["Information"]
                        </MudText>
                        <MudDivider Class="mb-3" />

                        <!-- Add your fields here -->
                        <MudText Typo="Typo.caption" Color="Color.Secondary">@LC["ID"]</MudText>
                        <MudText Typo="Typo.body2" Class="mb-2">@[Entity].Id</MudText>

                        <!-- Add more fields as needed -->
                    </MudPaper>
                </MudItem>
            </MudGrid>
        }
    </DialogContent>
    <DialogActions>
        <MudButton OnClick="Close" Color="Color.Primary" Variant="Variant.Filled">
            @LC["Close"]
        </MudButton>
    </DialogActions>
</MudDialog>

@code {
    [CascadingParameter]
    private MudDialogInstance? MudDialog { get; set; }

    [Parameter]
    public [GeneratedDtoType]? [Entity] { get; set; }

    private void Close() => MudDialog?.Close();
}
```

## After Creating Files

1. **Restore packages** (if you added Gridify for the first time):
   ```bash
   dotnet restore
   ```

2. **Regenerate API clients** so the Blazor app can use the new endpoint:
   ```bash
   dotnet run --generateclients true --project src/WebAPI/HeadStart.WebAPI.csproj
   ```

3. **Build and run** the application:
   ```bash
   dotnet run --project src/Aspire/AppHost
   ```

## Common Gridify Filter Operators

- `=` - Equals
- `!=` - Not equals
- `>` - Greater than
- `>=` - Greater than or equal
- `<` - Less than
- `<=` - Less than or equal
- `@=` - Contains (case-sensitive)
- `@=*` - Contains (case-insensitive)
- `^=` - Starts with
- `$=` - Ends with
- `()` - Grouping
- `,` - AND operator
- `|` - OR operator

## Example Filter Expressions

```
Name @=* John                           // Name contains "John" (case-insensitive)
Age > 18, Age < 65                      // Age between 18 and 65
Status = Active | Status = Pending      // Status is either Active or Pending
CreatedDate >= 2024-01-01               // Created after Jan 1, 2024
(Name @=* John | Name @=* Jane), Age > 25  // (Name contains John OR Jane) AND Age > 25
```
