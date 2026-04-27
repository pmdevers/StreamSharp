using Microsoft.EntityFrameworkCore;
using StreamSharp.PostgresSQL.Entities;

namespace StreamSharp.PostgresSQL;

public class StreamSharpDB(DbContextOptions<StreamSharpDB> options) : DbContext(options)
{
    public DbSet<EventRecord> Events { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<EventRecord>(e =>
        {
            e.ToTable("EventRecords");
            e.HasKey(x => x.EventId);
            e.Property(x => x.StreamId).IsRequired();
            e.Property(x => x.Version).IsRequired();
            e.Property(x => x.EventType).IsRequired();
            e.Property(x => x.EventData).IsRequired();
            e.Property(x => x.OccurredOn).IsRequired();
        });
    }
}
