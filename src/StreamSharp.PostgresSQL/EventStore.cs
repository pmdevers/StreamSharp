using Microsoft.EntityFrameworkCore;
using StreamSharp.Core.Abstractions;
using StreamSharp.PostgresSQL.Entities;
using System.Text.Json;

namespace StreamSharp.PostgresSQL;

public class EventStore<TId>(
    StreamSharpDB dBContext) 
    : IEventStore<TId>
    where TId : struct
{
    public async Task<EventStream<TId>> LoadAsync(TId id, CancellationToken cancellationToken = default)
    {
        var streamId = id.ToString()!;

        var records = dBContext.Events
                    .Where(x => x.StreamId == streamId)
                    .OrderBy(x => x.Version)
                    .ToListAsync(cancellationToken);

        var events = new List<DomainEvent>();
        foreach (var record in await records)
        {
            var type = Type.GetType(record.EventType);
            if (type == null)
                continue;
            var domainEvent = JsonSerializer.Deserialize(record.EventData, type) as DomainEvent;
            if (domainEvent != null)
                events.Add(domainEvent);
        }

        return EventStream<TId>.Create(id, [..events]);
    }

    public async Task<EventStream<TId>> SaveAsync(EventStream<TId> streamEvents, CancellationToken cancellationToken = default)
    {
        var streamId = streamEvents.Id.ToString()!;
        var uncommitted = streamEvents.GetUncommittedEvents();
        int version = streamEvents.Version;

        foreach (var domainEvent in uncommitted)
        {
            var type = domainEvent.GetType();
            var record = new EventRecord
            {
                EventId = domainEvent.MessageId,
                StreamId = streamId,
                Version = ++version,
                EventType = type.AssemblyQualifiedName!,
                EventData = JsonSerializer.Serialize(domainEvent, type),
                OccurredOn = domainEvent.OccurredOn,
            };
            dBContext.Events.Add(record);
        }

        await dBContext.SaveChangesAsync(cancellationToken);

        return await LoadAsync(streamEvents.Id, cancellationToken);
    }
}
