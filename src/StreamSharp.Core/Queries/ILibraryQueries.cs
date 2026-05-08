using StreamSharp.Core.Abstractions;
using StreamSharp.Core.Entities;

namespace StreamSharp.Core.Queries;

public interface ILibraryQueries
{
    public class LibraryDto
    {
        public LibraryId Id { get; set; }
        public required string Name { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
    }

    public class LibraryItemDto
    {
        public LibraryItemId Id { get; set; }
        public LibraryId LibraryId { get; set; }
        public required string Name { get; set; }
        public required string Type { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
    }

    Task<LibraryDto[]> GetLibrariesAsync(int page, int pageSize, string? search = null, string? sortBy = null, CancellationToken cancellationToken = default);
    Task<LibraryItemDto[]> GetLibraryItemsAsync(LibraryId libraryId, int page, int pageSize, string? search = null, string? sortBy = null, CancellationToken cancellationToken = default);
    Task<LibraryDto?> GetLibraryByIdAsync(LibraryId libraryId, CancellationToken cancellationToken);
}


