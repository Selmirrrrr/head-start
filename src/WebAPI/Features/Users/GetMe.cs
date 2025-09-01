using System.Security.Claims;
using FastEndpoints;
using HeadStart.WebAPI.Data;
using Microsoft.EntityFrameworkCore;

namespace HeadStart.WebAPI.Features.Users;

public static class GetMe
{
    public class Endpoint : EndpointWithoutRequest<UserProfile>
    {
        public required HeadStartDbContext DbContext { get; set; }

        public override void Configure()
        {
            Get("/users/me");
            Version(1);
        }

        public override async Task HandleAsync(CancellationToken ct)
        {
            if (Guid.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var userId)
                || !await DbContext.Users.AnyAsync(u => u.Id == userId, cancellationToken: ct))
            {
                await Send.NotFoundAsync(ct);
                return;
            }

            var profile = await DbContext.Users
                .Select(u => new UserProfile(u.Id,
                    u.Email,
                    u.UserTenantRoles.ToDictionary(utr => utr.TenantPath.ToString(), utr => utr.Role.Code),
                    u.LastSelectedTenantPath, u.LanguageCode, u.IsDarkMode, User.Claims))
                .SingleAsync(u => u.Id == userId, ct);

            await Send.OkAsync(profile, ct);
        }
    }

    public sealed record UserProfile(
    Guid Id,
    string UserName,
    Dictionary<string, string> AssignedRoles,
    string? LastSelectedTenantId = null,
    string? LanguageCode = null,
    bool IsDarkMode = false,
    IEnumerable<Claim> CLaims = null!);
}


