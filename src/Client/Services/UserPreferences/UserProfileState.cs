namespace HeadStart.Client.Services.UserPreferences;

/// <summary>
/// Implementation of IUserProfileState following Blazor state management best practices.
/// Uses immutable UserProfile snapshots with precise event notifications.
/// </summary>
public class UserProfileState(
    ILogger<UserProfileState> logger) : IUserProfileState, IDisposable
{
    // Cache refresh interval of 60 seconds
    private TimeSpan RefreshInterval => TimeSpan.FromSeconds(60);

    // Current user profile state (immutable snapshot)
    private UserProfile _currentValue = UserProfile.Empty;

    /// <summary>
    /// Gets the current user profile snapshot (immutable).
    /// </summary>
    public UserProfile Value => _currentValue;

    /// <summary>
    /// Event triggered when the user profile changes.
    /// Subscribers receive the new UserProfile snapshot.
    /// </summary>
    public event EventHandler<UserProfile>? Changed;

    /// <summary>
    /// Ensures the user profile is initialized for the given user ID.
    /// Only loads from database on first call or when user changes.
    /// </summary>
    public async Task EnsureInitializedAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            //  TODO - call API to load user data
            var result = UserProfile.Empty;
            if (result is not null)
            {
                SetInternal(result);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to initialize user profile");
            throw;
        }
    }

    /// <summary>
    /// Refreshes the user profile by clearing cache and reloading from API.
    /// </summary>
    public async Task RefreshAsync(CancellationToken cancellationToken = default)
    {
        // TODO - call API to refresh user data
    }

    /// <summary>
    /// Sets a new user profile directly (for local updates after database changes).
    /// </summary>
    public void Set(UserProfile userProfile)
    {
        ArgumentNullException.ThrowIfNull(userProfile);
        SetInternal(userProfile);
    }

    /// <summary>
    /// Updates specific fields locally without database access.
    /// </summary>
    public void UpdateLocal(
        string? profilePictureDataUrl = null,
        string? displayName = null,
        string? languageCode = null)
    {
        if (_currentValue == UserProfile.Empty)
        {
            return;
        }

        var updatedProfile = _currentValue with
        {
            ProfilePictureDataUrl = profilePictureDataUrl ?? _currentValue.ProfilePictureDataUrl,
            DisplayName = displayName ?? _currentValue.DisplayName,
            LanguageCode = languageCode ?? _currentValue.LanguageCode
        };

        SetInternal(updatedProfile);
    }

    private void SetInternal(UserProfile newProfile)
    {
        var oldProfile = _currentValue;
        _currentValue = newProfile;

        // Trigger event if profile actually changed
        if (!ReferenceEquals(oldProfile, newProfile))
        {
            Changed?.Invoke(this, newProfile);
        }
    }

    /// <summary>
    /// Updates the user's language code in the database and refreshes local state.
    /// </summary>
    public async Task SetLanguageAsync(string languageCode, CancellationToken cancellationToken = default)
    {
        // TODO - call API to update language code
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}
