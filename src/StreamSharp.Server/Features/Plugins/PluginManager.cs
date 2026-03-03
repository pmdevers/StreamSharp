using StreamSharp.Plugin;
using System.IO.Compression;
using System.Text.Json;

namespace StreamSharp.Server.Features.Plugins;

public class PluginManager(string pluginsPath)
{
    private readonly string _pluginRoot = pluginsPath;
    private readonly Dictionary<string, PluginEntry> _loaded = [];
    private bool _needsRestart = false;

    public List<string> GetPlugins()
         => [.. _loaded.Keys];

    public bool NeedsRestart => _needsRestart;

    public void ApplyServicesToBuilder(IServiceCollection services)
    {
        foreach (var entry in _loaded.Values)
        {
            entry.context.ApplyServices(services);
        }
    }

    public void ApplyEndpointsToApp(IEndpointRouteBuilder routeBuilder)
    {
        foreach (var entry in _loaded.Values)
        {
            entry.context.ApplyEndpoints(routeBuilder);
        }
    }

    public async Task<string> InstallPlugin(Stream zipStream)
    {
        string tempDir = Path.Combine(_pluginRoot, Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        using var archive = new ZipArchive(zipStream);
        await archive.ExtractToDirectoryAsync(tempDir);
        _needsRestart = true;
        return tempDir;
    }

    public void LoadAll()
    {
        foreach (var pluginDir in Directory.GetDirectories(_pluginRoot))
        {
            string configPath = Path.Combine(pluginDir, "plugin.json");
            if (!File.Exists(configPath))
            {
                Directory.Delete(pluginDir, true);
                continue;
            }

            LoadPlugin(pluginDir);
        }
    }

    public IPlugin LoadPlugin(string pluginDir)
    {
        string configPath = Path.Combine(pluginDir, "plugin.json");
        string json = File.ReadAllText(configPath);
        var config = JsonSerializer.Deserialize<PluginConfig>(json, JsonSerializerOptions.Web);

        var ctx = new PluginLoadContext(pluginDir);
        string assemblyPath = Path.Combine(pluginDir, config.EntryAssembly);

        var asm = ctx.LoadFromAssemblyPath(assemblyPath);
        var pluginType = asm.GetTypes().First(t => typeof(IPlugin).IsAssignableFrom(t));

        var instance = Activator.CreateInstance(pluginType) as IPlugin;
        var pluginContext = new PluginContext();

        var entry = new PluginEntry(ctx, instance, pluginContext, config.Name);

        _loaded[config.Name] = entry;

        instance.Start(pluginContext);

        return instance;
    }

    public void UninstallPlugin(string name)
    {
        if (!_loaded.TryGetValue(name, out var entry))
            return;

        // Store the plugin path before clearing references
        string pluginPath = entry.loadContext.PluginPath;

        // Stop the plugin
        entry.instance.Stop();

        // Create a weak reference to track when the context is actually collected
        var weakRef = new WeakReference(entry.loadContext);

        // Initiate unload
        entry.loadContext.Unload();

        // Remove from dictionary to clear strong reference
        _loaded.Remove(name);

        // Clear local references
        entry = default;

        // Wait for the AssemblyLoadContext to be garbage collected
        for (int i = 0; weakRef.IsAlive && i < 10; i++)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        try
        {
            // Delete the directory now that the assembly is unloaded
            Directory.Delete(pluginPath, recursive: true);
        }
        catch
        {

            // DLL Is still locked, likely due to a lingering reference. In a production system, consider logging this and retrying later.
        }


        // Mark that a restart is needed to remove services/endpoints
        _needsRestart = true;
    }

    public void ClearRestartFlag()
    {
        _needsRestart = false;
    }

}


public class PluginConfig
{
    public string Name { get; set; }
    public string Version { get; set; }
    public string EntryAssembly { get; set; }
}

internal record PluginEntry(
    PluginLoadContext loadContext,
    IPlugin instance,
    PluginContext context,
    string name
);

