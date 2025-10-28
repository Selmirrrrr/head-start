using FastEndpoints;
using HeadStart.WebAPI.Data;

namespace HeadStart.WebAPI.Features.PlatformAdmin.Tenants;

public static class TenantsGetList
{
    public class Endpoint : EndpointWithoutRequest<Response>
    {
        public required HeadStartDbContext DbContext { get; set; }

        public override void Configure()
        {
            Get("/tenants");
            Group<PlatformAdminEndpointGroup>();
            Version(1);
        }

        public override async Task HandleAsync(CancellationToken ct)
        {
            var tenants = DbContext.Tenants.Select(t => new TenantViewModel(t.Code.ToString(), t.Name));
            await Send.OkAsync(new Response([.. tenants]), ct);
        }
    }

    public record Response(IList<TenantViewModel> Tenants);

    public record TenantViewModel(string Id, string Name);
}
