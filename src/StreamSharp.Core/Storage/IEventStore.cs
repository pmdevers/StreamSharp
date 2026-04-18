using StreamSharp.Core.Abstractions;

namespace StreamSharp.Server.Features.Medialibrary;

public interface IEventStore<TId>
    where TId : struct
{
    Task<EventStream<TId>> LoadAsync(TId id, CancellationToken cancellationToken = default);
    Task<EventStream<TId>> SaveAsync(EventStream<TId> streamEvents, CancellationToken cancellationToken = default);
}
