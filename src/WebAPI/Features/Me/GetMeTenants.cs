using FastEndpoints;
using HeadStart.SharedKernel.Services;
using HeadStart.WebAPI.Data;
using HeadStart.WebAPI.Services;
using Microsoft.EntityFrameworkCore;

namespace HeadStart.WebAPI.Features.Me;

public static class GetMeTenants
{
    public class Endpoint : EndpointWithoutRequest<Response>
    {
        public required HeadStartDbContext DbContext { get; set; }
        public required ICurrentUserService CurrentUser { get; set; }

        public override void Configure()
        {
            Get("/tenants");
            Group<MeEndpointGroup>();
            Version(1);
        }

        public override async Task HandleAsync(CancellationToken ct)
        {
            var userId = CurrentUser.UserId;

            var user = await DbContext.Users.AnyAsync(u => u.Id == userId, ct);

            if (!user)
            {
                await Send.NotFoundAsync(ct);
                return;
            }

            var tenants = await DbContext.UserTenantRoles.Where(d => d.UserId == userId && d.DateDebutValidite <= DateTime.UtcNow && d.DateFinValidite >= DateTime.UtcNow).Select(d => new TenantViewModel(
                d.TenantPath.ToString(),
                d.Tenant.Name))
                .ToListAsync(ct);

            await Send.OkAsync(new Response([.. tenants]), ct);
        }
    }

    public record Response(IList<TenantViewModel> Tenants);

    public record TenantViewModel(string Id, string Name);
}
