using StreamSharp.Plugin;
using System.IO.Compression;
using System.Text.Json;

namespace StreamSharp.Server.Features.Plugins;

public class PluginManager(string pluginsPath)
{
    private readonly string _pluginRoot = pluginsPath;
    private readonly Dictionary<string, (PluginLoadContext ctx, IPlugin instance)> _loaded = [];

    public List<string> GetPlugins()
         => [.. _loaded.Keys];

    public async Task<string> InstallPlugin(Stream zipStream)
    {
        string tempDir = Path.Combine(_pluginRoot, Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        using var archive = new ZipArchive(zipStream);
        await archive.ExtractToDirectoryAsync(tempDir);
        return tempDir;
    }

    public void LoadAll()
    {
        foreach (var pluginDir in Directory.GetDirectories(_pluginRoot))
        {
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

        _loaded[config.Name] = (ctx, instance);
        return instance;
    }

    public void UninstallPlugin(string name)
    {
        if (!_loaded.TryGetValue(name, out var entry))
            return;

        // Store the plugin path before clearing references
        string pluginPath = entry.ctx.PluginPath;

        // Stop the plugin
        entry.instance.Stop();

        // Create a weak reference to track when the context is actually collected
        var weakRef = new WeakReference(entry.ctx);

        // Initiate unload
        entry.ctx.Unload();

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

        // Delete the directory now that the assembly is unloaded
        Directory.Delete(pluginPath, recursive: true);
    }

}


public class PluginConfig
{
    public string Name { get; set; }
    public string Version { get; set; }
    public string EntryAssembly { get; set; }
}


