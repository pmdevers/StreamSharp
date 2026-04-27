using StreamSharp.Core.Abstractions;
using StreamSharp.Core.Entities;

namespace StreamSharp.Core.Storage;

public class LibraryRepository(
    IEventStore<LibraryId> eventStore, 
    IEventBus eventBus)
{
    public async Task<Library> GetAsync(LibraryId id, CancellationToken cancellationToken = default)
    {
        var stream = await eventStore.LoadAsync(id, cancellationToken);
        return AggregateRoot.LoadFromHistory<Library, LibraryId>(stream);
    }
    public async Task SaveAsync(Library library, CancellationToken cancellationToken = default)
    {
        var stream = library.GetUncommittedEvents();

        await eventStore.SaveAsync(stream, cancellationToken);
        // Publish events to the bus
        foreach (var domainEvent in stream)
        {
            await eventBus.PublishAsync(domainEvent, cancellationToken);
        }
    }
}
