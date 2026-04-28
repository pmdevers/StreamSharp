using Microsoft.EntityFrameworkCore;
using StreamSharp.Core.Entities;
using StreamSharp.Core.Queries;

namespace StreamSharp.PostgreSQL.Queries;

internal class LibraryQueries(StreamSharpDB context) : ILibraryQueries
{
    public Task<ILibraryQueries.LibraryDto[]> GetLibrariesAsync(int page, int pageSize, string? search = null, string? sortBy = null, CancellationToken cancellationToken = default)
    {
        return context.Libraries
            .Where(x => search == null || x.Name.Contains(search))
            .OrderBy(x => sortBy == "name" ? x.Name : x.CreatedAt.ToString())
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new ILibraryQueries.LibraryDto { Id = x.Id, Name = x.Name, CreatedAt = x.CreatedAt })
            .ToArrayAsync(cancellationToken);
    }

    public Task<ILibraryQueries.LibraryDto?> GetLibraryByIdAsync(LibraryId libraryId, CancellationToken cancellationToken)
    {
        return context.Libraries
            .Where(x => x.Id == libraryId)
            .Select(x => new ILibraryQueries.LibraryDto { Id = x.Id, Name = x.Name, CreatedAt = x.CreatedAt })
            .FirstOrDefaultAsync(cancellationToken);
    }

    public Task<ILibraryQueries.LibraryItemDto[]> GetLibraryItemsAsync(LibraryId libraryId, int page, int pageSize, string? search = null, string? sortBy = null, CancellationToken cancellationToken = default)
    {
        return context.LibraryItems
            .Where(x => x.LibraryId == libraryId)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new ILibraryQueries.LibraryItemDto { Id = x.Id, LibraryId = x.LibraryId, Name = x.Name, Type = "test", CreatedAt = x.CreatedAt })
            .ToArrayAsync(cancellationToken);
    }
}
