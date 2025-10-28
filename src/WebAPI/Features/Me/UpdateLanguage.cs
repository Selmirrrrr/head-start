using FastEndpoints;
using HeadStart.SharedKernel.Services;
using HeadStart.WebAPI.Data;
using HeadStart.WebAPI.Services;
using Microsoft.EntityFrameworkCore;

namespace HeadStart.WebAPI.Features.Me;

public static class UpdateLanguage
{
    public class Request
    {
        public required string LanguageCode { get; set; }
    }

    public class Response
    {
        public string LanguageCode { get; set; } = string.Empty;
    }

    public class Endpoint : Endpoint<Request, Response>
    {
        public required HeadStartDbContext DbContext { get; set; }
        public required ICurrentUserService CurrentUser { get; set; }

        public override void Configure()
        {
            Patch("/language");
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

            user.LanguageCode = req.LanguageCode;
            await DbContext.SaveChangesAsync(ct);

            await Send.OkAsync(new Response { LanguageCode = user.LanguageCode }, ct);
        }
    }
}
