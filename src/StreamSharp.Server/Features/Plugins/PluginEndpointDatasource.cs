using Microsoft.Extensions.Primitives;

namespace StreamSharp.Server.Features.Plugins;

public class PluginEndpointDatasource : EndpointDataSource
{
    private readonly List<Endpoint> _endpoints = [];
    private readonly List<IChangeToken> _changeTokens = [];

    public void AddEndpoint(Endpoint endpoint)
    {
        _endpoints.Add(endpoint);
    }

    public override IReadOnlyList<Endpoint> Endpoints => _endpoints;

    public override IChangeToken GetChangeToken()
        => new CompositeChangeToken(_changeTokens);
}
