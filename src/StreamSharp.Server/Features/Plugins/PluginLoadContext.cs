using System.Reflection;
using System.Runtime.Loader;

namespace StreamSharp.Server.Features.Plugins;

public class PluginLoadContext(string pluginPath)
    : AssemblyLoadContext(isCollectible: true)
{
    private readonly string[] _assemblies = [
        "StreamSharp.Core",
        "StreamSharp.Plugin"
        ];

    public string PluginPath => pluginPath;

    protected override Assembly Load(AssemblyName assemblyName)
    {
        if (_assemblies.Contains(assemblyName.Name))
        {
            return null!;
        }

        var assemblyPath = Path.Combine(pluginPath, $"{assemblyName.Name}.dll");
        return File.Exists(assemblyPath)
            ? LoadFromAssemblyPath(assemblyPath)
            : null!;
    }
}
