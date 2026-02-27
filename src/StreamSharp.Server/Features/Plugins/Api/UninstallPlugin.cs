using Microsoft.AspNetCore.Mvc;

namespace StreamSharp.Server.Features.Plugins.Api;

public static class UninstallPlugin
{
    internal static async Task<IResult> Handle(
        string name,
        [FromServices] PluginManager pluginManager
        )
    {
        pluginManager.UninstallPlugin(name);

        return TypedResults.Ok(new { Message = "Plugin uninstalled successfully" });
    }
}
