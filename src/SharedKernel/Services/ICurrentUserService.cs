namespace HeadStart.SharedKernel.Services;

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
    string[]? PlatformRoles { get; }
}
