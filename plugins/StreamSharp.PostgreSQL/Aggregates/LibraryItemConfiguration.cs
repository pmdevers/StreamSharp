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
        var libraryConverter = new ValueConverter<LibraryId, Guid>(
            id => (Guid)id,
            value => LibraryId.From(value));

        var libraryItemConverter = new ValueConverter<LibraryItemId, Guid>(
            id => (Guid)id,
            value => LibraryItemId.From(value));

        builder.ToTable("LibraryItems");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasConversion(libraryItemConverter);
        builder.Property(x => x.LibraryId).HasConversion(libraryConverter);
        builder.Property(x => x.CreatedAt).IsRequired();
    }
}

