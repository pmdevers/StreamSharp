using StreamSharp.Plugin;
using System.Reflection;
using System.Runtime.Loader;

namespace StreamSharp.Server.Features.Plugins;

public class PluginLoadContext(string pluginPath)
    : AssemblyLoadContext(isCollectible: true)
{
    public string PluginPath => pluginPath;

    protected override Assembly Load(AssemblyName assemblyName)
    {
        if (assemblyName.Name == typeof(IPlugin).Assembly.GetName().Name)
        {
            return null;
        }

        var assemblyPath = Path.Combine(pluginPath, $"{assemblyName.Name}.dll");
        return File.Exists(assemblyPath)
            ? LoadFromAssemblyPath(assemblyPath)
            : null!;
    }
}
