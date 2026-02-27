using Microsoft.AspNetCore.Mvc;

namespace StreamSharp.Server.Features.Server;

public static class ServerApi
{
    extension(WebApplication app)
    {
        public void MapServerApi()
        {
            var group = app.MapGroup("/server").WithTags("Server Api");

            group.MapGet("/restart", async ([FromServices] ServerHost server) => await server.StopAsync())
                .WithName("RestartServer")
                .WithDescription("Restarts the server.");
            group.MapGet("/shutdown", async ([FromServices] ServerHost server) => await server.ServerStopAsync())
                .WithName("ShutdownServer")
                .WithDescription("Shuts down the server.");
        }
    }
}
