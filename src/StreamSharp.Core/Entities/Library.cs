using StreamSharp.Core.Abstractions;
using StreamSharp.Server.Features.Medialibrary.Events;

namespace StreamSharp.Core.Entities;

public record LibraryItemCreatedEvent(LibraryItemId id, string Name, string Path) : StreamEvent;

[GenerateId]
public class Library() : AggregateRoot<LibraryId>(LibraryId.New())
{
    public string Name { get; private set; } = string.Empty;

    public IReadOnlyCollection<LibraryItem> Items { get; private set; } = [];

    public static Library Create(string name)
    {
        var library = new Library();
        library.RecordEvent(new LibraryCreatedEvent(name));
        return library;
    }

    public LibraryItem CreateItem(string name, string path)
    {
        var item = LibraryItem.Create(this, name, path);
        RecordEvent(new LibraryItemCreatedEvent(item.Id, name, path));
        return item;
    }

    public void Apply(LibraryCreatedEvent @event)
    {
        Name = @event.Name;
    }

    public void Apply(LibraryItemCreatedEvent @event)
    {

        var item = new LibraryItem(@event.Id, @event.Name, @event.Path);
        Items = Items.Append(item).ToArray();
    }
}
