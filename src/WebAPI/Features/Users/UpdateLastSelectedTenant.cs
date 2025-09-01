using System.Security.Claims;
using FastEndpoints;
using HeadStart.WebAPI.Data;
using HeadStart.WebAPI.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace HeadStart.WebAPI.Features.Users;

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
            Patch("/users/me/tenant");
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
                .FirstOrDefaultAsync(u => u.Id == userId, ct);

            if (user == null)
            {
                await Send.NotFoundAsync(ct);
                return;
            }

            // Validate that the user has access to this tenant
            if (!string.IsNullOrEmpty(req.LastSelectedTenantPath))
            {
                var hasAccess = user.Droits.Any(utr =>
                    utr.TenantPath.ToString() == req.LastSelectedTenantPath);

                if (!hasAccess)
                {
                    AddError("User does not have access to this tenant");
                    await Send.ErrorsAsync(403, ct);
                    return;
                }

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
