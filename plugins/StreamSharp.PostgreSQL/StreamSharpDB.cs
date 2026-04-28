using Microsoft.EntityFrameworkCore;
using StreamSharp.Core.Abstractions;
using StreamSharp.PostgreSQL.Aggregates;
using static StreamSharp.Core.Queries.ILibraryQueries;

namespace StreamSharp.PostgreSQL;

public partial class StreamSharpDB(DbContextOptions<StreamSharpDB> options) : DbContext(options), IUnitOfWork
{
    public DbSet<EventDocument> Events { get; set; }
    public DbSet<LibraryDto> Libraries { get; set; }
    public DbSet<LibraryItemDto> LibraryItems { get; set; }

    public IRepository<TAggregate, TId> GetRepository<TAggregate, TId>()
        where TAggregate : AggregateRoot<TId>
        where TId : struct
        => new AggregateRepository<TAggregate, TId>(this);

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new EventDocumentConfiguration());
        modelBuilder.ApplyConfiguration(new LibraryConfiguration());
        modelBuilder.ApplyConfiguration(new LibraryItemConfiguration());
    }
}

public class AggregateRepository<TAggregate, TId>(StreamSharpDB context) : IRepository<TAggregate, TId>
    where TAggregate : AggregateRoot<TId>
    where TId : struct
{
    public async void Add(TAggregate entity)
    {
        var version = 0;

        foreach (var item in entity.GetUncommittedEvents())
        {
            version++;
            context.Events.Add(new EventDocument
            {
                Id = Guid.NewGuid(),
                AggregateId = entity.Id.ToString(),
                Version = version,
                Type = EventSerializer.GetTypeName(item),
                Data = EventSerializer.Serialize(item),
                CreatedAt = TimeProvider.System.GetUtcNow(),
            });
        }
    }

    public void Delete(TAggregate entity)
    {
        // Event-sourced aggregates aren't typically deleted
    }

    public async Task<TAggregate?> TryFindAsync(TId id, CancellationToken cancellationToken = default)
    {
        // Create a DbSet-backed event collection
        var eventCollection = new DbSetEventCollection<TId>(id, context.Events);

        // Load events from database
        await eventCollection.LoadAsync(cancellationToken);

        if (eventCollection.Count == 0)
            return null;

        // Create and hydrate aggregate
        var aggregate = AggregateRoot.Create<TAggregate, TId>(id);
        aggregate.LoadFromHistory(eventCollection);

        return aggregate;
    }
}
