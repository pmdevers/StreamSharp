using Microsoft.Extensions.DependencyInjection;
using StreamSharp.Core.Abstractions;

namespace StreamSharp.PostgreSQL;

public class Plugin : IPlugin
{
    public string Name => "PostgreSQL Plugin";
    public string Description => "Enable Storage in PostgreSQL";

    public void Start(IPluginContext context)
    {
        context.RegisterServices(services =>
        {
            services.AddPostgreSQL();

            services.AddHostedService<MigrationService>();
        });

        context.RegisterApi(routes =>
        {
        });

    }

    public void Stop()
    {
        
    }
}
