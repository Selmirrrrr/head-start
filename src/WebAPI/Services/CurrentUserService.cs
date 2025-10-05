using System.Security.Claims;

namespace HeadStart.WebAPI.Services;

/// <summary>
/// Service that provides access to the current authenticated user's information from JWT claims.
/// Values are lazily loaded and cached per request scope.
/// </summary>
public sealed class CurrentUserService(IHttpContextAccessor httpContextAccessor)
{
    private Guid? _userId;
    private string? _email;
    private string? _givenName;
    private string? _surname;

    private ClaimsPrincipal User => httpContextAccessor.HttpContext?.User
                                    ?? throw new InvalidOperationException("No user context available");

    public bool IsAuthenticated => httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;

    public Guid UserId => _userId ??= Guid.Parse(
        User.FindFirst(ClaimTypes.NameIdentifier)?.Value
        ?? throw new InvalidOperationException("User ID claim is missing"));

    public string Email => _email ??= User.FindFirst(ClaimTypes.Email)?.Value
        ?? throw new InvalidOperationException("Email claim is missing");

    public string GivenName => _givenName ??= User.FindFirst(ClaimTypes.GivenName)?.Value
        ?? throw new InvalidOperationException("Given name claim is missing");

    public string Surname => _surname ??= User.FindFirst(ClaimTypes.Surname)?.Value
        ?? throw new InvalidOperationException("Surname claim is missing");
}
