namespace StreamSharp.Server.Features.Plugins.Tracking;

internal class TrackingEndpointRouteBuilder(IEndpointRouteBuilder inner, List<Endpoint> tracking) : IEndpointRouteBuilder
{
    private readonly TrackingEndpointDataSource _trackingDataSource = new(tracking);
    private bool _initialized;

    public IServiceProvider ServiceProvider => inner.ServiceProvider;

    public ICollection<EndpointDataSource> DataSources
    {
        get
        {
            if (!_initialized)
            {
                inner.DataSources.Add(_trackingDataSource);
                _initialized = true;
            }
            return inner.DataSources;
        }
    }

    public IApplicationBuilder CreateApplicationBuilder() => inner.CreateApplicationBuilder();
}
