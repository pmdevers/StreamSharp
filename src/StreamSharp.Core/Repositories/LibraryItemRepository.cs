using StreamSharp.Core.Abstractions;
using StreamSharp.Core.Entities;

namespace StreamSharp.Core.Storage;

public class LibraryItemRepository(
    IEventStore<LibraryItemId> eventStore,
    IEventBus eventBus)
{
    public async Task<LibraryItem> GetAsync(LibraryItemId id, CancellationToken cancellationToken = default)
    {
        var stream = await eventStore.LoadAsync(id, cancellationToken);
        return AggregateRoot.LoadFromHistory<LibraryItem, LibraryItemId>(stream);
    }
    public async Task SaveAsync(LibraryItem libraryItem, CancellationToken cancellationToken = default)
    {
        var stream = libraryItem.GetUncommittedEvents();

        await eventStore.SaveAsync(stream, cancellationToken);
        // Publish events to the bus
        foreach (var domainEvent in stream)
        {
            await eventBus.PublishAsync(domainEvent, cancellationToken);
        }
    }
}
