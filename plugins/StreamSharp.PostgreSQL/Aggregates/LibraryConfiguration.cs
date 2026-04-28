using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using StreamSharp.Core.Entities;

namespace StreamSharp.PostgreSQL.Aggregates;

internal class LibraryConfiguration : IEntityTypeConfiguration<Library>
{
    public void Configure(EntityTypeBuilder<Library> builder)
    {
        var converter = new ValueConverter<LibraryId, Guid>(
            id => id.Value,
            value => new LibraryId(value));

        builder.ToTable("Libraries");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasConversion(converter);
        builder.Property(x => x.Name).IsRequired();
        builder.Property(x => x.CreatedAt).IsRequired();
    }
}

