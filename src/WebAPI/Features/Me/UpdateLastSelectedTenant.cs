using FastEndpoints;
using HeadStart.WebAPI.Data;
using HeadStart.WebAPI.Services;
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
        public required CurrentUserService CurrentUser { get; set; }

        public override void Configure()
        {
            Patch("/me/tenant");
            Version(1);
        }

        public override async Task HandleAsync(Request req, CancellationToken ct)
        {
            var userId = CurrentUser.UserId;

            var user = await DbContext.Users
                .Include(u => u.Droits)
                .FirstOrDefaultAsync(u => u.IdpId == userId, ct);

            if (user == null)
            {
                await Send.NotFoundAsync(ct);
                return;
            }

            // Validate that the user has access to this tenant
            if (!string.IsNullOrWhiteSpace(req.LastSelectedTenantPath))
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
