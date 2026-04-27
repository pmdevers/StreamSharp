using StreamSharp.Core.Abstractions;
using StreamSharp.Server.Features.Medialibrary.Events;

namespace StreamSharp.Core.Entities;

[GenerateId]
public class Library() : AggregateRoot<LibraryId>(LibraryId.New())
{
    public string Name { get; private set; } = string.Empty;

    public static Library Create(string name)
    {
        var library = new Library();
        library.RecordEvent(new LibraryCreated(name));
        return library;
    }

    public LibraryItem CreateItem(string path)
    {
        return LibraryItem.Create(this, path);
    }

    public void Apply(LibraryCreated @event)
    {
        Name = @event.Name;
    }
}
