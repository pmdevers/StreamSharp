//using Microsoft.EntityFrameworkCore;
//using StreamSharp.Core.EventSourcing;
//using StreamSharp.PostgresSQL.EventSourcing;
//using System.Text.Json;

//namespace StreamSharp.PostgresSQL;

//internal class SharpStreamDBContext : DbContext
//{
//    public SharpStreamDBContext(DbContextOptions<SharpStreamDBContext> options)
//        : base(options)
//    {
//    }

//    public DbSet<EventEntity> Events => Set<EventEntity>();

//    protected override void OnModelCreating(ModelBuilder modelBuilder)
//    {
//        modelBuilder.Entity<EventEntity>(entity =>
//        {
//            entity.ToTable("events");
//            entity.HasKey(e => e.Id);
//            entity.Property(e => e.StreamId).IsRequired();
//            entity.Property(e => e.Version).IsRequired();
//            entity.Property(e => e.EventType).IsRequired();
//            entity.Property(e => e.EventData).IsRequired();
//            entity.Property(e => e.OccouredOn).IsRequired();
//            entity.HasIndex(e => new { e.StreamId, e.Version }).IsUnique();
//        });
//    }

//    public async Task<IReadOnlyList<EventRecord>> LoadStreamAsync(Guid streamId, CancellationToken ct = default)
//    {
//        var entities = await Events
//            .Where(e => e.StreamId == streamId)
//            .OrderBy(e => e.Version)
//            .ToArrayAsync(ct);

//        var events = entities
//            .Select(Deserialize)
//            .OfType<EventRecord>()
//            .ToArray();

//        return events;
//    }

//    //public async Task SaveEventAsync(EventRecord @event, CancellationToken ct = default)
//    //{
//    //    var entity = new EventEntity
//    //    {
//    //        StreamId = @event.StreamId,
//    //        Version = @event.Version,
//    //        EventType = @event.GetType().AssemblyQualifiedName!,
//    //        EventData = JsonSerializer.Serialize(@event, @event.GetType()),
//    //        OccouredOn = @event.OccouredOn,
//    //    };

//    //    await Events.AddAsync(entity, ct);
//    //    await SaveChangesAsync(ct);
//    //}

//    private static EventRecord? Deserialize(EventEntity entity)
//    {
//        var type = Type.GetType(entity.EventType);
//        if (type is null)
//            return null;

//        return (EventRecord?)JsonSerializer.Deserialize(entity.EventData, type);
//    }
//}
