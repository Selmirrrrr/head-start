using FastEndpoints;
using HeadStart.WebAPI.Data;
using HeadStart.WebAPI.Services;
using Microsoft.EntityFrameworkCore;

namespace HeadStart.WebAPI.Features.Me;

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
        public required ICurrentUserService CurrentUser { get; set; }

        public override void Configure()
        {
            Patch("/dark-mode");
            Group<MeEndpointGroup>();
            Version(1);
        }

        public override async Task HandleAsync(Request req, CancellationToken ct)
        {
            var userId = CurrentUser.UserId;

            var user = await DbContext.Users.FirstOrDefaultAsync(u => u.Id == userId, ct);
            if (user == null)
            {
                await Send.NotFoundAsync(ct);
                return;
            }

            user.DarkMode = req.IsDarkMode;
            await DbContext.SaveChangesAsync(ct);

            await Send.OkAsync(new Response { IsDarkMode = user.DarkMode }, ct);
        }
    }
}
