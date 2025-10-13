using FastEndpoints;

namespace HeadStart.WebAPI.Features.Absences;

public sealed class AbsencesEndpointGroup : Group
{
        public AbsencesEndpointGroup()
        {
            Configure(
                "absences",
                ep =>
                {
                    ep.Description(x => x.Produces(401).WithTags("absences"));
                });
        }
}
