using StreamSharp.Core.Entities;

namespace StreamSharp.Core.Queries;

public interface ILibraryQueries
{
    public record LibraryDto(Guid Id, string Name, DateTimeOffset CreatedAt);
    Task<LibraryDto[]> GetLibrariesAsync(int page, int pageSize, string? search = null, string? sortBy = null, CancellationToken cancellationToken = default);
    Task<LibraryDto?> GetLibraryByIdAsync(LibraryId libraryId, CancellationToken cancellationToken);
}


