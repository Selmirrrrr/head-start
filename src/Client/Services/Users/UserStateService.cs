using HeadStart.Client.Generated;
using HeadStart.Client.Generated.Models;
using HeadStart.SharedKernel.Models.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HeadStart.Client.Services.Users;

// Singleton service to hold the actual state
public class UserStateService
{
    private UserState _currentState = UserState.Default;
    private readonly object _lock = new();
    
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
public class UserStateContainer  
{
    private readonly ApiClientV1 _apiClient;
    private readonly UserStateService _stateService;

    public event Action? OnStateChanged
    {
        add => _stateService.OnStateChanged += value;
        remove => _stateService.OnStateChanged -= value;
    }

    public UserStateContainer(ApiClientV1 apiClient, UserStateService stateService)
    {
        _apiClient = apiClient;
        _stateService = stateService;
    }

    public UserState CurrentState => _stateService.CurrentState;
    public bool IsInitialized => _stateService.IsInitialized;

    public async Task InitializeAsync()
    {
        if (_stateService.IsInitialized)
        {
            return; // Already initialized
        }

        try
        {
            var userProfile = await _apiClient.Api.V1.Users.Me.GetAsync();
            
            if (userProfile == null)
            {
                _stateService.CurrentState = UserState.Default;
                _stateService.IsInitialized = true;
                return;
            }

            _stateService.CurrentState = new UserState
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
                DernierTenantSelectionne = userProfile.DernierTenantSelectionnePath
            };

            _stateService.IsInitialized = true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to initialize user state: {ex.Message}");
            _stateService.CurrentState = UserState.Default;
            _stateService.IsInitialized = true;
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
            var request = new HeadStartWebAPIFeaturesUsersUpdateDarkMode_Request
            {
                IsDarkMode = isDarkMode
            };

            await _apiClient.Api.V1.Users.Me.DarkMode.PatchAsync(request);
            
            _stateService.CurrentState = _stateService.CurrentState with { DarkMode = isDarkMode };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to update dark mode: {ex.Message}");
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
            var request = new HeadStartWebAPIFeaturesUsersUpdateLanguage_Request
            {
                LanguageCode = languageCode
            };

            await _apiClient.Api.V1.Users.Me.Language.PatchAsync(request);
            
            _stateService.CurrentState = _stateService.CurrentState with { LangueCode = languageCode };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to update language: {ex.Message}");
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
            var request = new HeadStartWebAPIFeaturesUsersUpdateLastSelectedTenant_Request
            {
                LastSelectedTenantPath = tenantPath
            };

            await _apiClient.Api.V1.Users.Me.Tenant.PatchAsync(request);
            
            _stateService.CurrentState = _stateService.CurrentState with { DernierTenantSelectionne = tenantPath };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to update selected tenant: {ex.Message}");
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