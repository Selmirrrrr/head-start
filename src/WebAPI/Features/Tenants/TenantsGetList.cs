using FastEndpoints;
using HeadStart.WebAPI.Data;

namespace HeadStart.WebAPI.Features.Tenants;

public static class TenantsGetList
{
    public class Endpoint : EndpointWithoutRequest<Response>
    {
        public HeadStartDbContext DbContext { get; set; }

        public override void Configure()
        {
            Get("/api/tenants");
            Scopes("profile");
        }

        public override async Task HandleAsync(CancellationToken ct)
        {
            var tenants = DbContext.Tenants.Select(t => new TenantViewModel(t.Id.ToString(), t.Name));
            await Send.OkAsync(new Response([.. tenants]), ct);
        }
    }

    public record Response(IList<TenantViewModel> Tenants);

    public record TenantViewModel(string Id, string Name);
}
