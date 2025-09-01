using System.Security.Claims;
using FastEndpoints;
using HeadStart.WebAPI.Data;
using Microsoft.EntityFrameworkCore;

namespace HeadStart.WebAPI.Features.Users;

public static class UpdateDarkMode
{
    public class Request
    {
        public bool IsDarkMode { get; set; }
    }

    public class Response
    {
        public bool IsDarkMode { get; set; }
    }

    public class Endpoint : Endpoint<Request, Response>
    {
        public required HeadStartDbContext DbContext { get; set; }

        public override void Configure()
        {
            Patch("/users/me/dark-mode");
            Version(1);
        }

        public override async Task HandleAsync(Request req, CancellationToken ct)
        {
            if (!Guid.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var userId))
            {
                await Send.UnauthorizedAsync(ct);
                return;
            }

            var user = await DbContext.Users.FirstOrDefaultAsync(u => u.Id == userId, ct);
            if (user == null)
            {
                await Send.NotFoundAsync(ct);
                return;
            }

            user.IsDarkMode = req.IsDarkMode;
            await DbContext.SaveChangesAsync(ct);

            await Send.OkAsync(new Response { IsDarkMode = user.IsDarkMode }, ct);
        }
    }
}