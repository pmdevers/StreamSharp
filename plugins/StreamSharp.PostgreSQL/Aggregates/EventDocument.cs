using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using StreamSharp.Core.Abstractions;

namespace StreamSharp.PostgreSQL.Aggregates;

[GenerateId]
public class EventDocument
{
    public Guid Id { get; set; }
    public AggregateId AggregateId { get; set; }
    public string AggregateName { get; set; } = null!;
    public int Version { get; set; }
    public string Type { get; set; } = null!;
    public string Data { get; set; } = null!;
    public DateTimeOffset CreatedAt { get; set; }
}

internal class EventDocumentConfiguration : IEntityTypeConfiguration<EventDocument>
{
    public void Configure(EntityTypeBuilder<EventDocument> builder)
    {

        var converter = new ValueConverter<AggregateId, Guid>(
           id => id.Value,
           value => AggregateId.From(value));

        builder.ToTable(nameof(EventDocument));

        builder.HasKey(e => e.Id);
        builder.Property(e => e.AggregateId)
            .HasConversion(converter)
            .IsRequired();
        builder.Property(e => e.AggregateName).IsRequired();
        builder.Property(e => e.Version).IsRequired();
        builder.Property(e => e.Type).IsRequired();
        builder.Property(e => e.Data).IsRequired();
        builder.Property(e => e.CreatedAt).IsRequired();
    }
}
