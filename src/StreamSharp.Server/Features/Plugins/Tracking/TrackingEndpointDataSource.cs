using Microsoft.Extensions.Primitives;

namespace StreamSharp.Server.Features.Plugins.Tracking;

internal class TrackingEndpointDataSource(List<Endpoint> tracking) : EndpointDataSource
{
    public override IReadOnlyList<Endpoint> Endpoints
    {
        get
        {
            tracking.Clear();
            return tracking;
        }
    }

    public override IChangeToken GetChangeToken() => new CancellationChangeToken(CancellationToken.None);
}
