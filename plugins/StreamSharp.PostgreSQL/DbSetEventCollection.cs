using Microsoft.EntityFrameworkCore;
using StreamSharp.Core.Abstractions;
using StreamSharp.PostgreSQL.Aggregates;

namespace StreamSharp.PostgreSQL;

/// <summary>
/// DbSet-backed implementation of IEventCollection for event sourcing
/// </summary>
/// <typeparam name="TId">The type of the aggregate ID</typeparam>
public class DbSetEventCollection<TId>(TId aggregateId, DbSet<EventDocument> events, TimeProvider? timeProvider = null) : IEventCollection<TId>
    where TId : struct
{
    private readonly DbSet<EventDocument> _events = events;
    private readonly List<DomainEvent> _uncommitted = [];
    private DomainEvent[] _committed = [];
    private int _loadedVersion;
    private DateTimeOffset? _firstEventCreatedAt;
    private DateTimeOffset? _lastEventCreatedAt;

    public TId AggregateId { get; } = aggregateId;

    public int Version => _loadedVersion;

    public int ExpectedVersion => Version + _uncommitted.Count;

    public DateTimeOffset? CreatedOn => _firstEventCreatedAt;

    public DateTimeOffset? LastModifiedOn => _lastEventCreatedAt;

    public int Count => _committed.Length;

    /// <summary>
    /// Load events from the database
    /// </summary>
    public async Task LoadAsync(CancellationToken cancellationToken = default)
    {
        var aggregateIdString = AggregateId.ToString();
        var eventDocs = await _events
            .Where(x => x.AggregateId == aggregateIdString)
            .OrderBy(x => x.Version)
            .ToArrayAsync(cancellationToken);

        _committed = eventDocs
            .Select(doc => EventSerializer.Deserialize(doc.Data, doc.Type))
            .Where(e => e != null)
            .ToArray()!;

        _loadedVersion = _committed.Length;

        if (eventDocs.Length > 0)
        {
            _firstEventCreatedAt = eventDocs[0].CreatedAt;
            _lastEventCreatedAt = eventDocs[^1].CreatedAt;
        }
    }

    public void Append(DomainEvent e)
    {
        _uncommitted.Add(e);

        var now = (timeProvider ?? TimeProvider.System).GetUtcNow();

        var aggregateIdString = AggregateId.ToString();
        var version = _loadedVersion + _uncommitted.Count - 1;

        _events.Add(new EventDocument
        {
            Id = Guid.NewGuid(),
            AggregateId = aggregateIdString,
            Version = version,
            Type = EventSerializer.GetTypeName(e),
            Data = EventSerializer.Serialize(e),
            CreatedAt = now
        });

        // Update timestamps
        _lastEventCreatedAt = now;
        if (!_firstEventCreatedAt.HasValue)
        {
            _firstEventCreatedAt = now;
        }
    }

    public IEventCollection<TId> GetUncommittedEvents()
    {
        var collection = new DbSetEventCollection<TId>(AggregateId, _events);
        collection._committed = [.. _uncommitted];
        collection._loadedVersion = _uncommitted.Count;
        return collection;
    }

    public IEnumerator<DomainEvent> GetEnumerator()
        => _committed.ToList().GetEnumerator();

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        => GetEnumerator();
}
