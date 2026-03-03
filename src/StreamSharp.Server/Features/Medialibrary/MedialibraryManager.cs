using System.Collections.Concurrent;

namespace StreamSharp.Server.Features.Medialibrary;

public class MedialibraryManager
{
    private readonly ConcurrentDictionary<LibraryId, Library> _libraries = [];
    private readonly ConcurrentDictionary<LibraryItemId, LibraryItem> _libraryItems = [];

    public async Task<Library> CreateAsync(Library library)
    {
        return _libraries.AddOrUpdate(library.Id, library, (_, _) => library);
    }

    public async Task<LibraryItem> CreateAsync(LibraryItem item)
    {
        return _libraryItems.AddOrUpdate(item.Id, item, (_, _) => item);
    }

    public async Task<List<Library>> FindLibraries()
    {
        return [.. _libraries.Select(x => x.Value)];
    }

    public async Task<Library?> FindById(LibraryId id)
        => _libraries.GetValueOrDefault(id);

    public async Task<List<LibraryItem>> FindByItemsId(LibraryId id)
    {
        return [.. _libraryItems
            .Where(x => x.Value.LibraryId == id)
            .Select(x => x.Value)
        ];
    }
}
