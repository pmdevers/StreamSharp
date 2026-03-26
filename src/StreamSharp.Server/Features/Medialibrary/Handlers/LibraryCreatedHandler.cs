namespace StreamSharp.Server.Features.Medialibrary.Handlers;

public class LibraryCreatedHandler(IEventBus eventBus) : IMessageHandler<LibraryCreated>
{
    public Task HandleAsync(LibraryCreated message, CancellationToken ct = default)
    {
        Console.WriteLine("Created " + message.Id);

        return Task.CompletedTask;
    }
}


public class LibraryCreatedHandler2(IEventBus eventBus) : IMessageHandler<LibraryCreated>
{
    public async Task HandleAsync(LibraryCreated message, CancellationToken ct = default)
    {
        await Task.Delay(5000, ct);

        Console.WriteLine("Created 2" + message.Id);
    }
}
