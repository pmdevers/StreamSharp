using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using StreamSharp.Core.Abstractions;
using StreamSharp.Core.Entities;
using static StreamSharp.Core.Queries.ILibraryQueries;

namespace StreamSharp.PostgreSQL.Aggregates;

internal class LibraryConfiguration : IEntityTypeConfiguration<LibraryDto>
{
    public void Configure(EntityTypeBuilder<LibraryDto> builder)
    {
        var converter = new ValueConverter<LibraryId, Guid>(
            id => (Guid)id,
            value => LibraryId.From(value));

        builder.ToTable("Libraries");
        builder.HasKey(x => x.Id);  
        builder.Property(x => x.Id).HasConversion(converter);
        builder.Property(x => x.Name).IsRequired();
        builder.Property(x => x.CreatedAt).IsRequired();
    }
}

