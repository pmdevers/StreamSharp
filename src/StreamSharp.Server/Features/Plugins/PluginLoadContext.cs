using System.Reflection;
using System.Runtime.Loader;

namespace StreamSharp.Server.Features.Plugins;

public class PluginLoadContext(string pluginPath)
    : AssemblyLoadContext(isCollectible: true)
{
    public string PluginPath => pluginPath;

    protected override Assembly Load(AssemblyName assemblyName)
    {
        if (assemblyName.Name == "StreamSharp.Plugin")
        {
            return null;
        }

        var assemblyPath = Path.Combine(Path.GetDirectoryName(pluginPath)!, $"{assemblyName.Name}.dll");
        return File.Exists(assemblyPath)
            ? LoadFromAssemblyPath(assemblyPath)
            : null!;
    }
}
