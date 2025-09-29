using System.Security.Claims;
using FastEndpoints;
using HeadStart.WebAPI.Data;
using Microsoft.EntityFrameworkCore;

namespace HeadStart.WebAPI.Features.Me;

public static class UpdateLastSelectedTenant
{
    public class Request
    {
        public string? LastSelectedTenantPath { get; set; }
    }

    public class Response
    {
        public string? LastSelectedTenantPath { get; set; }
    }

    public class Endpoint : Endpoint<Request, Response>
    {
        public required HeadStartDbContext DbContext { get; set; }

        public override void Configure()
        {
            Patch("/me/tenant");
            Version(1);
        }

        public override async Task HandleAsync(Request req, CancellationToken ct)
        {
            if (!Guid.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var userId))
            {
                await Send.UnauthorizedAsync(ct);
                return;
            }

            var user = await DbContext.Users
                .Include(u => u.Droits)
                .FirstOrDefaultAsync(u => u.IdpId == userId, ct);

            if (user == null)
            {
                await Send.NotFoundAsync(ct);
                return;
            }

            // Validate that the user has access to this tenant
            if (!string.IsNullOrEmpty(req.LastSelectedTenantPath))
            {
                user.DernierTenantSelectionneId = new LTree(req.LastSelectedTenantPath);
            }
            else
            {
                user.DernierTenantSelectionneId = null;
            }

            await DbContext.SaveChangesAsync(ct);

            await Send.OkAsync(new Response { LastSelectedTenantPath = user.DernierTenantSelectionneId?.ToString() }, ct);
        }
    }
}
