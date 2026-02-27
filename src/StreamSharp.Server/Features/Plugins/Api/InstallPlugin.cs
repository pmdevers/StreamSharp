using Microsoft.AspNetCore.Mvc;

namespace StreamSharp.Server.Features.Plugins.Api;

public static class InstallPlugin
{
    internal static async Task<IResult> Handle(
        IFormFile pluginFile,
        [FromServices] PluginManager pluginManager
        )
    {
        using var stream = pluginFile.OpenReadStream();
        var plugin = await pluginManager.InstallPlugin(stream);
        pluginManager.LoadPlugin(plugin);
        return TypedResults.Ok(plugin);
    }
}
