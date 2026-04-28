using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace StreamSharp.PostgreSQL.Aggregates;

[GenerateId]
public class EventDocument
{
    public Guid Id { get; set; }
    public string AggregateId { get; set; }
    public int Version { get; set; }
    public string Type { get; set; } = null!;
    public string Data { get; set; } = null!;
    public DateTimeOffset CreatedAt { get; set; }
}

internal class EventDocumentConfiguration : IEntityTypeConfiguration<EventDocument>
{
    public void Configure(EntityTypeBuilder<EventDocument> builder)
    {
        builder.ToTable(nameof(EventDocument));

        builder.HasKey(e => e.Id);
        builder.Property(e => e.AggregateId).IsRequired();
        builder.Property(e => e.Version).IsRequired();
        builder.Property(e => e.Type).IsRequired();
        builder.Property(e => e.Data).IsRequired();
        builder.Property(e => e.CreatedAt).IsRequired();
    }
}
