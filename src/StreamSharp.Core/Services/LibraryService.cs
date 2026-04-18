using StreamSharp.Core.Abstractions;
using StreamSharp.Core.Entities;
using StreamSharp.Server.Features.Medialibrary;

namespace StreamSharp.Core.Services;

public class LibraryService(
    IEventStore<LibraryId> store,
    IEventBus eventBus)
{

    public Library CreateLibrary(string name)
    {
        var libraryId = LibraryId.New();
        var library = Library.Create(name);


    }
}
