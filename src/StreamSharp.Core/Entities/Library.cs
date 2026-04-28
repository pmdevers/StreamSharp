using StreamSharp.Core.Abstractions;
using StreamSharp.Core.Events;

namespace StreamSharp.Core.Entities;

[GenerateId]
public class Library : AggregateRoot<LibraryId>
{
    private Library(LibraryId id) : base(id)
    {
    }

    public string Name { get; private set; } = string.Empty;

    public static Library Create(string name)
    {
        var library = new Library(LibraryId.New());
        library.RecordEvent(new LibraryCreated(library.Id, name));
        return library;
    }

    public LibraryItem CreateItem(string path)
    {
        return LibraryItem.Create(this, path);
    }

    internal void Apply(LibraryCreated e)
    {
       Name = e.Name;
    }
}
