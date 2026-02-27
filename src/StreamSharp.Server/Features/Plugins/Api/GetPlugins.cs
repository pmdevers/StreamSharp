using Microsoft.AspNetCore.Mvc;

namespace StreamSharp.Server.Features.Plugins.Api;

public static class GetPlugins
{
    internal static async Task<IResult> Handle(
        [FromServices] PluginManager pluginManager
        )
    {
        var result = pluginManager.GetPlugins();
        return Results.Ok(result);
    }
}


