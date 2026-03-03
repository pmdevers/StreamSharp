using Microsoft.AspNetCore.Mvc;

namespace StreamSharp.Server.Features.Plugins.Api;

public static class UninstallPlugin
{
    internal static async Task<IResult> Handle(
        string name,
        [FromServices] PluginManager pluginManager,
        [FromServices] ServerHost serverHost
        )
    {
        pluginManager.UninstallPlugin(name);

        // Trigger server restart to remove plugin services and endpoints
        _ = Task.Run(async () =>
        {
            await Task.Delay(1000); // Give time for response to be sent
            await serverHost.StopAsync(); // This will trigger restart if EnableRestartOnFailure is true
        });

        return TypedResults.Ok(new 
        { 
            Message = "Plugin uninstalled successfully. Server will restart to remove plugin services and endpoints.",
            RequiresRestart = true 
        });
    }
}
