using FastEndpoints;
using HeadStart.SharedKernel.Models.Constants;

namespace HeadStart.WebAPI.Features.PlatformAdmin;

public sealed class PlatformAdminEndpointGroup : Group
{
    public PlatformAdminEndpointGroup()
    {
        Configure(
            "platform-admin",
            ep =>
            {
                ep.Description(x => x
                    .Produces(401)
                    .Produces(403)
                    .WithTags("platform-admin")
                    .WithDisplayName("PlatformAdmin"));

                // Require platform-admin role for all endpoints in this group
                ep.Roles(RoleName.PlatformAdmin);
            });
    }
}
