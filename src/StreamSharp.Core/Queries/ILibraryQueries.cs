using StreamSharp.Core.Abstractions;
using StreamSharp.Core.Entities;

namespace StreamSharp.Core.Queries;

public interface ILibraryQueries
{
    public class LibraryDto
    {
        public AggregateId Id { get; set; }
        public required string Name { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
    }

    public class LibraryItemDto
    {
        public AggregateId Id { get; set; }
        public AggregateId LibraryId { get; set; }
        public required string Name { get; set; }
        public required string Type { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
    }

    Task<LibraryDto[]> GetLibrariesAsync(int page, int pageSize, string? search = null, string? sortBy = null, CancellationToken cancellationToken = default);
    Task<LibraryItemDto[]> GetLibraryItemsAsync(AggregateId libraryId, int page, int pageSize, string? search = null, string? sortBy = null, CancellationToken cancellationToken = default);
    Task<LibraryDto?> GetLibraryByIdAsync(AggregateId libraryId, CancellationToken cancellationToken);
}


