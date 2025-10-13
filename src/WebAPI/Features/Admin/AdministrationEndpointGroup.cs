using FastEndpoints;

namespace HeadStart.WebAPI.Features.Admin;

public sealed class AdministrationEndpointGroup : Group
{
        public AdministrationEndpointGroup()
        {
            Configure(
                "admin",
                ep =>
                {
                    ep.Description(x => x.Produces(401).WithTags("admin"));
                });
        }
}
