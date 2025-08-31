namespace HeadStart.Client.Services.UserPreferences;

/// <summary>
/// Immutable user profile record representing user information state.
/// </summary>
public sealed record UserProfile(
    string UserId,
    string UserName,
    string Email,
    string ProfilePictureDataUrl = "/img/avatar.png",
    string? DisplayName = "Demo User",
    string? DefaultRole = "Senior Claims Manager",
    string[]? AssignedRoles = null,
    string? TenantId = null,
    string? TenantName = null,
    string? LanguageCode = null
)
{
    /// <summary>
    /// Creates an empty user profile with default values.
    /// </summary>
    public static UserProfile Empty => new(
        UserId: Guid.CreateVersion7().ToString(),
        UserName: "Selmir",
        Email: "demo@user.ch"
    );

    /// <summary>
    /// Checks if the user has the specified role.
    /// </summary>
    public bool IsInRole(string role) => AssignedRoles?.Contains(role) ?? false;
}
