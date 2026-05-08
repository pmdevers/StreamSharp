using Microsoft.Extensions.DependencyInjection;

namespace StreamSharp.Core.Abstractions;

public interface IPlugin
{
    public string Name { get; }
    public string Description { get; }

    void Start(IPluginContext context);
    void Stop();
}

public static class ServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddWhenNotRegisterd<T>(Action<IServiceCollection> register)
        {
            if (services.Any(x => x.ServiceType == typeof(T)))
            {
                return services;
            }

            register(services);
            return services;
        }
    }
}
