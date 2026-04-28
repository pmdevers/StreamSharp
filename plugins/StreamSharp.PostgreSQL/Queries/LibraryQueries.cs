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
            .Select(x => new ILibraryQueries.LibraryDto(x.Id, x.Name, x.CreatedAt))
            .ToArrayAsync(cancellationToken);
    }

    public Task<ILibraryQueries.LibraryDto?> GetLibraryByIdAsync(LibraryId libraryId, CancellationToken cancellationToken)
    {
        return context.Libraries
            .Where(x => x.Id == libraryId)
            .Select(x => new ILibraryQueries.LibraryDto(x.Id, x.Name, x.CreatedAt))
            .FirstOrDefaultAsync(cancellationToken);
    }
}
