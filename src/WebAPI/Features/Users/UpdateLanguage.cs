using System.Security.Claims;
using FastEndpoints;
using HeadStart.WebAPI.Data;
using Microsoft.EntityFrameworkCore;

namespace HeadStart.WebAPI.Features.Users;

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

        public override void Configure()
        {
            Patch("/users/me/language");
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

            // Validate language code (basic validation for common codes)
            var validLanguageCodes = new[] { "en", "fr", "de", "es", "it", "pt", "nl", "pl", "ru", "ja", "zh" };
            if (!validLanguageCodes.Contains(req.LanguageCode.ToLower()))
            {
                AddError("Invalid language code");
                await Send.ErrorsAsync(400, ct);
                return;
            }

            user.LanguageCode = req.LanguageCode;
            await DbContext.SaveChangesAsync(ct);

            await Send.OkAsync(new Response { LanguageCode = user.LanguageCode }, ct);
        }
    }
}