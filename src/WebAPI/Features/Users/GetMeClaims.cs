using FastEndpoints;
using HeadStart.WebAPI.Data;

namespace HeadStart.WebAPI.Features.Users;

public static class GetMeClaims
{
    public class Endpoint : EndpointWithoutRequest<IEnumerable<Claim>>
    {
        public required HeadStartDbContext DbContext { get; set; }

        public override void Configure()
        {
            Get("/users/me/claims");
            Version(1);
        }

        public override async Task HandleAsync(CancellationToken ct)
        {
            await Send.OkAsync(User.Claims.Select(c => new Claim(c.Type, c.Value)), ct);
        }
    }

    public record class Claim(string Type, string Value);
}


