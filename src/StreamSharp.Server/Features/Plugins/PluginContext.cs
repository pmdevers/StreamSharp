using StreamSharp.Plugin;
using StreamSharp.Server.Features.Plugins.Tracking;
using System.Text.Encodings.Web;

namespace StreamSharp.Server.Features.Plugins;

internal class PluginContext(IPlugin plugin) : IPluginContext
{
    private readonly List<Action<IServiceCollection>> _serviceRegistrations = [];
    private readonly List<Action<IEndpointRouteBuilder>> _endpointRegistrations = [];
    private readonly List<ServiceDescriptor> _registeredServices = [];
    private readonly List<Endpoint> _registeredEndpoints = [];

    public IReadOnlyList<ServiceDescriptor> RegisteredServices => _registeredServices;
    public IReadOnlyList<Endpoint> RegisteredEndpoints => _registeredEndpoints;

    public void RegisterServices(Action<IServiceCollection> services)
    {
        _serviceRegistrations.Add(services);
    }

    public void RegisterApi(Action<IEndpointRouteBuilder> routes)
    {
        _endpointRegistrations.Add(routes);
    }

    internal void ApplyServices(IServiceCollection services)
    {
        var trackingCollection = new TrakingServiceCollection(services, _registeredServices);
        foreach (var registration in _serviceRegistrations)
        {
            registration(trackingCollection);
        }
    }

    internal void ApplyEndpoints(IEndpointRouteBuilder routeBuilder)
    {
        var trackingBuilder = new TrackingEndpointRouteBuilder(routeBuilder, _registeredEndpoints);

        var group = trackingBuilder
            .MapGroup(UrlEncoder.Default.Encode(plugin.Name.ToLowerInvariant()))
            .WithTags(plugin.Name);

        foreach (var registration in _endpointRegistrations)
        {
            registration(group);
        }
    }
}
