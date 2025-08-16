using FastEndpoints;

namespace HeadStart.WebAPI.Features.Users;

public class GetMe
{
    public class Endpoint : EndpointWithoutRequest<Response>
    {
        public override void Configure()
        {
            Get("/api/users/me");
            AllowAnonymous();
        }

        public override async Task HandleAsync(CancellationToken ct)
        {
            var claims = User.Claims.Select(c => new ClaimsViewModel(c.Type, c.Value));
            await Send.OkAsync(new Response([.. claims]), ct);
        }
    }

    public record Response(IList<ClaimsViewModel> Claims);

    public record ClaimsViewModel(string Type, string Value);
}


