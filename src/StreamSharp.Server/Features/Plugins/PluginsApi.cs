using StreamSharp.Server.Features.Plugins.Api;

namespace StreamSharp.Server.Features.Plugins;

public static class PluginsApi
{
    extension(WebApplication app)
    {
        public void MapPluginsApi()
        {
            var group = app.MapGroup("/plugins").WithTags("Plugins Api");
            group.MapGet("/", GetPlugins.Handle)
                .WithName("GetPlugins")
                .WithDescription("Retrieves a list of plugins.");
            group.MapPost("/install", InstallPlugin.Handle)
                .DisableAntiforgery()
                .WithName("InstallPlugin")
                .WithDescription("Installs a new plugin from a provided URL.");
            group.MapPost("/uninstall", UninstallPlugin.Handle)
                .WithName("UninstallPlugin")
                .WithDescription("Uninstalls an existing plugin by its name.");
        }
    }
}
