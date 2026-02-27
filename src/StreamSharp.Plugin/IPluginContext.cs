using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace StreamSharp.Plugin;

public interface IPluginContext
{
    void RegisterServices(Action<IServiceCollection> services);
    void RegisterApi(Action<IEndpointRouteBuilder> routes);
}
