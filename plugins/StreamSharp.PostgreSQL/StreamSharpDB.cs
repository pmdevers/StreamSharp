using Microsoft.EntityFrameworkCore;
using StreamSharp.Core.Abstractions;
using StreamSharp.PostgreSQL.Aggregates;

using static StreamSharp.Core.Queries.ILibraryQueries;

namespace StreamSharp.PostgreSQL;

public partial class StreamSharpDB(DbContextOptions<StreamSharpDB> options) : DbContext(options), IUnitOfWork
{
    private readonly List<Aggregate> _trackedAggregates = [];

    public DbSet<EventDocument> Events { get; set; }
    public DbSet<LibraryDto> Libraries { get; set; }
    public DbSet<LibraryItemDto> LibraryItems { get; set; }

    public IRepository<TAggregate> GetRepository<TAggregate>()
        where TAggregate : Aggregate
        => new AggregateRepository<TAggregate>(this);

    internal void TrackAggregate(Aggregate aggregate)
    {
        _trackedAggregates.Add(aggregate);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var aggregate in _trackedAggregates)
        {
            var events = aggregate.GetUncommittedEvents();
            var version = events.Version;
            foreach (var evt in events)
            {
                var eventDoc = new EventDocument
                {
                    AggregateId = aggregate.Id,
                    AggregateName = aggregate.GetType().Name,
                    Data = EventSerializer.Serialize(evt),
                    Type = EventSerializer.GetTypeName(evt),
                    Version = version++,
                    CreatedAt = evt.OccurredOn
                };
                Events.Add(eventDoc);
            }
        }
        _trackedAggregates.Clear();

        return await base.SaveChangesAsync(cancellationToken);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new EventDocumentConfiguration());
        modelBuilder.ApplyConfiguration(new LibraryConfiguration());
        modelBuilder.ApplyConfiguration(new LibraryItemConfiguration());
    }
}

public class AggregateRepository<TAggregate>(StreamSharpDB context) : IRepository<TAggregate>
    where TAggregate : Aggregate
{
    public void Add(TAggregate entity)
    {
        context.TrackAggregate(entity);
    }

    public void Delete(TAggregate entity)
    {
        // Event-sourced aggregates aren't typically deleted
    }

    public async Task<TAggregate?> TryFindAsync(AggregateId id, CancellationToken cancellationToken = default)
    {
        var aggregateTypeName = typeof(TAggregate).Name;
        var events = await context.Events
            .Where(e => e.AggregateId.Equals(id) 
                     && e.AggregateName == aggregateTypeName)
            .Select(doc => EventSerializer.Deserialize(doc.Data, doc.Type))
            .ToArrayAsync(cancellationToken);

        if (events.Length == 0)
            return null;

        var eventCollection = EventCollection.Create(id, [.. events]);
        var aggregate = AggregateRoot.Create<TAggregate>(id);
        aggregate.LoadFromHistory(eventCollection);

        return aggregate;
    }
}
