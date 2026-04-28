using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using StreamSharp.Core.Entities;

namespace StreamSharp.PostgreSQL.Aggregates;

internal class LibraryItemConfiguration : IEntityTypeConfiguration<LibraryItem>
{
    public void Configure(EntityTypeBuilder<LibraryItem> builder)
    {
        var converter = new ValueConverter<LibraryItemId, Guid>(
            id => id.Value,
            value => new LibraryItemId(value));


        var libraryIdConverter = new ValueConverter<LibraryId, Guid>(
            id => id.Value,
            value => new LibraryId(value));

        builder.ToTable("LibraryItems");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasConversion(converter);
        builder.Property(x => x.LibraryId).HasConversion(libraryIdConverter).IsRequired();
        builder.Property(x => x.Path).IsRequired();
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.MetaData)
            .HasColumnType("jsonb")
            .HasConversion(
                v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                v => System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(v, (System.Text.Json.JsonSerializerOptions?)null)!);
    }
}

