using FastEndpoints;

namespace HeadStart.WebAPI.Features.Me;

public sealed class MeEndpointGroup : Group
{
        public MeEndpointGroup()
        {
            Configure(
                "me",
                ep =>
                {
                    ep.Description(x => x.Produces(401).WithTags("me"));
                });
        }
}
