using Microsoft.EntityFrameworkCore;
using StreamSharp.Core.Abstractions;
using StreamSharp.Core.Entities;
using StreamSharp.PostgreSQL.Aggregates;

namespace StreamSharp.PostgreSQL;

public partial class StreamSharpDB(DbContextOptions<StreamSharpDB> options) : DbContext(options), IUnitOfWork
{
    public DbSet<Library> Libraries { get; set; }
    public DbSet<LibraryItem> LibraryItems { get; set; }

    public IRepository<TAggregate, TId> GetRepository<TAggregate, TId>()
        => (IRepository<TAggregate, TId>)this;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new LibraryConfiguration());
        modelBuilder.ApplyConfiguration(new LibraryItemConfiguration());
    }
}

public partial class StreamSharpDB : IRepository<Library, LibraryId>
{
    public void Add(Library entity)
        => Libraries.Add(entity);

    public void Delete(Library entity)
        => Libraries.Remove(entity);

    public Task<Library?> TryFindAsync(LibraryId id, CancellationToken cancellationToken = default)
        => Libraries.FirstOrDefaultAsync(x=>x.Id == id, cancellationToken);
}

public partial class StreamSharpDB : IRepository<LibraryItem, LibraryItemId>
{
    public void Add(LibraryItem entity)
        => LibraryItems.Add(entity);

    public void Delete(LibraryItem entity)
        => LibraryItems.Remove(entity);

    public Task<LibraryItem?> TryFindAsync(LibraryItemId id, CancellationToken cancellationToken = default)
        => LibraryItems.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
}
