using HeadStart.Client.Generated;
using HeadStart.Client.Generated.Models;
using HeadStart.SharedKernel.Models.Constants;

namespace HeadStart.Client.Services.Users;

// Singleton service to hold the actual state
public class UserStateService
{
    private UserState _currentState = UserState.Default;
    private readonly Lock _lock = new();

    public event Action? OnStateChanged;

    public UserState CurrentState
    {
        get
        {
            lock (_lock)
            {
                return _currentState;
            }
        }
        set
        {
            lock (_lock)
            {
                _currentState = value;
            }
            OnStateChanged?.Invoke();
        }
    }

    public bool IsInitialized { get; set; }
}

// Scoped service that handles API calls
public class UserStateContainer(ApiClientV1 apiClient, UserStateService stateService, ILogger<UserStateContainer> logger)
{
    public event Action? OnStateChanged
    {
        add => stateService.OnStateChanged += value;
        remove => stateService.OnStateChanged -= value;
    }

    public UserState CurrentState => stateService.CurrentState;
    public bool IsInitialized => stateService.IsInitialized;

    public async Task InitializeAsync()
    {
        if (stateService.IsInitialized)
        {
            return; // Already initialized
        }

        try
        {
            var userProfile = await apiClient.Api.V1.Me.GetAsync();

            if (userProfile == null)
            {
                stateService.CurrentState = UserState.Default;
                stateService.IsInitialized = true;
                return;
            }

            stateService.CurrentState = new UserState
            {
                Id = Guid.TryParse(userProfile.Id, out var userId) ? userId : Guid.Empty,
                Email = userProfile.Email ?? string.Empty,
                Prenom = userProfile.Prenom ?? string.Empty,
                Nom = userProfile.Nom ?? string.Empty,
                Droits = userProfile.Roles?.Select(r => new Droit(
                    r.TenantPath ?? string.Empty,
                    r.RoleCode ?? string.Empty)).ToList() ?? new List<Droit>(),
                LangueCode = userProfile.LangueCode ?? LanguesCodes.Français,
                DarkMode = userProfile.DarkMode ?? false,
                DernierTenantSelectionne = userProfile.DernierTenantSelectionnePath,
                PlatformRoles = userProfile.PlatformRoles?.ToArray() ?? []
            };

            stateService.IsInitialized = true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to initialize user state");
        }
    }

    public async Task SetDarkModeAsync(bool isDarkMode)
    {
        if (!IsInitialized)
        {
            return;
        }

        try
        {
            var request = new HeadStartWebAPIFeaturesMeUpdateDarkMode_Request
            {
                IsDarkMode = isDarkMode
            };

            await apiClient.Api.V1.Me.DarkMode.PatchAsync(request);

            stateService.CurrentState = stateService.CurrentState with { DarkMode = isDarkMode };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to update dark mode");
        }
    }

    public async Task SetLanguageAsync(string languageCode)
    {
        if (!IsInitialized)
        {
            return;
        }

        try
        {
            var request = new HeadStartWebAPIFeaturesMeUpdateLanguage_Request()
            {
                LanguageCode = languageCode
            };

            await apiClient.Api.V1.Me.Language.PatchAsync(request);


            stateService.CurrentState = stateService.CurrentState with { LangueCode = languageCode };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to update language");
        }
    }

    public async Task SetLastSelectedTenantAsync(string? tenantPath)
    {
        if (!IsInitialized)
        {
            return;
        }

        try
        {
            var request = new HeadStartWebAPIFeaturesMeUpdateLastSelectedTenant_Request
            {
                LastSelectedTenantPath = tenantPath
            };

            await apiClient.Api.V1.Me.Tenant.PatchAsync(request);

            stateService.CurrentState = stateService.CurrentState with { DernierTenantSelectionne = tenantPath };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to update selected tenant");
        }
    }
}

public sealed record UserState
{
    public Guid Id { get; init; }
    public string Email { get; init; } = string.Empty;
    public string Prenom { get; init; } = string.Empty;
    public string Nom { get; init; } = string.Empty;
    public List<Droit> Droits { get; init; } = new();
    public string LangueCode { get; init; } = LanguesCodes.Français;
    public bool DarkMode { get; init; }
    public string? DernierTenantSelectionne { get; init; }
    public string[] PlatformRoles { get; init; } = [];
    public static UserState Default => new()
    {
        Id = Guid.Empty,
        Email = string.Empty,
        Prenom = string.Empty,
        Nom = string.Empty,
        Droits = new List<Droit>(),
        LangueCode = LanguesCodes.Français,
        DarkMode = false,
        DernierTenantSelectionne = null
    };
}

public sealed record Droit(string TenantPath, string RoleCode);
