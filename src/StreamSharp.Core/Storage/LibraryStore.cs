using StreamSharp.Core.Entities;

namespace StreamSharp.Server.Features.Medialibrary;


public interface ILibraryStore
{
    public Task<IEnumerable<Library>> GetLibrariesAsync(
        int page = 1,
        int pageSize = 10,
        string? search = null,
        string? sortBy = null,
        CancellationToken ct = default);

    public Task SaveAsync(Library library, CancellationToken ct = default);
    public Task<Library> LoadAsync(LibraryId id, CancellationToken ct = default);
}
