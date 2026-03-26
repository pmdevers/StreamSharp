using System.Collections.Concurrent;

namespace StreamSharp.Server.Features.Medialibrary;

public record LibraryCreated(LibraryId Id) : Message;
public record LibraryItemCreated(LibraryItemId id) : Message;

public class MedialibraryManager(IEventBus eventBus)
{
    private readonly ConcurrentDictionary<LibraryId, Library> _libraries = [];
    private readonly ConcurrentDictionary<LibraryItemId, LibraryItem> _libraryItems = [];

    public async Task<Library> CreateAsync(Library library)
    {
        var result = _libraries.AddOrUpdate(library.Id, library, (_, _) => library);
        await eventBus.PublishAsync(new LibraryCreated(result.Id));
        return result;
    }

    public async Task<LibraryItem> CreateAsync(LibraryItem item)
    {
        var result = _libraryItems.AddOrUpdate(item.Id, item, (_, _) => item);
        await eventBus.PublishAsync(new LibraryItemCreated(result.Id));
        return result;
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
