using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using StreamSharp.Core.Abstractions;
using System.Reflection;

namespace StreamSharp.UI;

public class WebApp : IPlugin
{
    public string Name => "StreamSharp Web Interface";

    public string Description => "Web Interface of StreamSharp";

    public static IFileProvider CreateFileProvider()
    {
        var assembly = Assembly.GetExecutingAssembly();
        return new ManifestEmbeddedFileProvider(assembly, "dist");
    }

    public void Start(IPluginContext context)
    {
        context.RegisterServices(services => { 
        
            services.AddSingleton(CreateFileProvider());
        });
    }

    public void Stop()
    {
    }
}
