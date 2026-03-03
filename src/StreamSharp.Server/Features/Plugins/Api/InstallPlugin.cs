using Microsoft.AspNetCore.Mvc;

namespace StreamSharp.Server.Features.Plugins.Api;

public static class InstallPlugin
{
    internal static async Task<IResult> Handle(
        IFormFile pluginFile,
        [FromServices] PluginManager pluginManager,
        [FromServices] ServerHost serverHost
        )
    {
        using var stream = pluginFile.OpenReadStream();
        var pluginPath = await pluginManager.InstallPlugin(stream);
        var plugin = pluginManager.LoadPlugin(pluginPath);

        // Trigger server restart to apply plugin services and endpoints
        _ = Task.Run(async () =>
        {
            await Task.Delay(1000); // Give time for response to be sent
            await serverHost.StopAsync(); // This will trigger restart if EnableRestartOnFailure is true
        });

        return TypedResults.Ok(new 
        { 
            Name = plugin.Name,
            Description = plugin.Description,
            Message = "Plugin installed successfully. Server will restart to apply plugin services and endpoints.",
            RequiresRestart = true
        });
    }
}
