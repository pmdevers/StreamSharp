using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using StreamSharp.Core.Entities;
using static StreamSharp.Core.Queries.ILibraryQueries;

namespace StreamSharp.PostgreSQL.Aggregates;

internal class LibraryItemConfiguration : IEntityTypeConfiguration<LibraryItemDto>
{
    public void Configure(EntityTypeBuilder<LibraryItemDto> builder)
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
        builder.Property(x => x.LibraryId).HasConversion(libraryIdConverter);
        builder.Property(x => x.CreatedAt).IsRequired();
    }
}

