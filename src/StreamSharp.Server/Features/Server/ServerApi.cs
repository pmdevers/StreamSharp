using Microsoft.AspNetCore.Mvc;

namespace StreamSharp.Server.Features.Server;

public static class ServerApi
{
    extension(WebApplication app)
    {
        public async Task MapServerApi()
        {
            var group = app.MapGroup("/server").WithTags("Server Api");

            group.MapGet("/restart", async ([FromServices] ServerHost server) => await server.StopAsync())
                .WithName("RestartServer")
                .WithDescription("Restarts the server.");
            group.MapGet("/shutdown", async ([FromServices] ServerHost server) => await server.ServerStopAsync())
                .WithName("ShutdownServer")
                .WithDescription("Shuts down the server.");


            var bus = app.Services.GetRequiredService<IEventBus>();
            await bus.PublishAsync(new ServerStartedMessage());
        }
    }
}

public record ServerStartedMessage() : Message
{
    public long Timestamp { get; init; } = TimeProvider.System.GetTimestamp();
}



public class TestHandler : IMessageHandler<ServerStartedMessage>
{
    public Task HandleAsync(ServerStartedMessage message, CancellationToken ct = default)
    {
        Console.WriteLine($"Server started at {message.Timestamp}");
        return Task.CompletedTask;
    }
}
