using System.Security.Claims;

namespace HeadStart.WebAPI.Services;

public interface ICurrentUserService
{
    Guid UserId { get; }
    string? SelectedTenantPath { get; }
    bool IsAuthenticated { get; }
    bool IsImpersonated { get; }
    Guid? ImpersonatedByUserId { get; }
    string Email { get; }
    string GivenName { get; }
    string Surname { get; }
    string[] PlatformRoles { get; }
}

/// <summary>
/// Service that provides access to the current authenticated user's information from JWT claims.
/// Values are lazily loaded and cached per request scope.
/// </summary>
public sealed class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    private Guid? _userId;
    private string? _email;
    private string? _givenName;
    private string? _surname;
    private bool _isImpersonated;
    private Guid? _impersonatedByUserId;
    private string? _selectedTenantPath;
    private string[] _platformRoles;
    private ClaimsPrincipal User => httpContextAccessor.HttpContext?.User
                                    ?? throw new InvalidOperationException("No user context available");

    public bool IsAuthenticated => httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;
    public bool IsImpersonated => _isImpersonated; // TODO Implement impersonation

    public Guid UserId => _userId ??= Guid.Parse(
        User.FindFirst(ClaimTypes.NameIdentifier)?.Value
        ?? throw new InvalidOperationException("User ID claim is missing"));

    public string Email => _email ??= User.FindFirst(ClaimTypes.Email)?.Value
        ?? throw new InvalidOperationException("Email claim is missing");

    public string GivenName => _givenName ??= User.FindFirst(ClaimTypes.GivenName)?.Value
        ?? throw new InvalidOperationException("Given name claim is missing");

    public string Surname => _surname ??= User.FindFirst(ClaimTypes.Surname)?.Value
        ?? throw new InvalidOperationException("Surname claim is missing");

    public string? SelectedTenantPath => _selectedTenantPath ??= httpContextAccessor.HttpContext?.Request.Headers["X-Tenant-Path"].ToString();

    public Guid? ImpersonatedByUserId => _impersonatedByUserId;

    public string[] PlatformRoles => _platformRoles ??= User.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value).ToArray() ?? [];
}
