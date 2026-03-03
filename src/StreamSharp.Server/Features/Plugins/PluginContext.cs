using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using StreamSharp.Plugin;

namespace StreamSharp.Server.Features.Plugins;

internal class PluginContext : IPluginContext
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
        var trackingCollection = new ServiceCollectionTracker(services, _registeredServices);
        foreach (var registration in _serviceRegistrations)
        {
            registration(trackingCollection);
        }
    }

    internal void ApplyEndpoints(IEndpointRouteBuilder routeBuilder)
    {
        var trackingBuilder = new EndpointRouteBuilderTracker(routeBuilder, _registeredEndpoints);
        foreach (var registration in _endpointRegistrations)
        {
            registration(trackingBuilder);
        }
    }

    private class ServiceCollectionTracker : IServiceCollection
    {
        private readonly IServiceCollection _inner;
        private readonly List<ServiceDescriptor> _tracking;

        public ServiceCollectionTracker(IServiceCollection inner, List<ServiceDescriptor> tracking)
        {
            _inner = inner;
            _tracking = tracking;
        }

        public ServiceDescriptor this[int index]
        {
            get => _inner[index];
            set => _inner[index] = value;
        }

        public int Count => _inner.Count;
        public bool IsReadOnly => _inner.IsReadOnly;

        public void Add(ServiceDescriptor item)
        {
            _tracking.Add(item);
            _inner.Add(item);
        }

        public void Clear() => _inner.Clear();
        public bool Contains(ServiceDescriptor item) => _inner.Contains(item);
        public void CopyTo(ServiceDescriptor[] array, int arrayIndex) => _inner.CopyTo(array, arrayIndex);
        public IEnumerator<ServiceDescriptor> GetEnumerator() => _inner.GetEnumerator();
        public int IndexOf(ServiceDescriptor item) => _inner.IndexOf(item);
        public void Insert(int index, ServiceDescriptor item)
        {
            _tracking.Add(item);
            _inner.Insert(index, item);
        }
        public bool Remove(ServiceDescriptor item) => _inner.Remove(item);
        public void RemoveAt(int index) => _inner.RemoveAt(index);
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }

    private class EndpointRouteBuilderTracker : IEndpointRouteBuilder
    {
        private readonly IEndpointRouteBuilder _inner;
        private readonly List<Endpoint> _tracking;

        public EndpointRouteBuilderTracker(IEndpointRouteBuilder inner, List<Endpoint> tracking)
        {
            _inner = inner;
            _tracking = tracking;
        }

        public IServiceProvider ServiceProvider => _inner.ServiceProvider;

        public ICollection<EndpointDataSource> DataSources
        {
            get
            {
                var trackingDataSource = new TrackingEndpointDataSource(_tracking);
                var collection = new List<EndpointDataSource>(_inner.DataSources) { trackingDataSource };
                return collection;
            }
        }

        public IApplicationBuilder CreateApplicationBuilder() => _inner.CreateApplicationBuilder();
    }

    private class TrackingEndpointDataSource : EndpointDataSource
    {
        private readonly List<Endpoint> _tracking;

        public TrackingEndpointDataSource(List<Endpoint> tracking)
        {
            _tracking = tracking;
        }

        public override IReadOnlyList<Endpoint> Endpoints
        {
            get
            {
                _tracking.Clear();
                return _tracking;
            }
        }

        public override IChangeToken GetChangeToken() => new CancellationChangeToken(CancellationToken.None);
    }
}
