using StreamSharp.Core.Abstractions;
using StreamSharp.Core.Events;

namespace StreamSharp.Core.Entities;

[GenerateId]
public class LibraryItem : AggregateRoot<LibraryItemId>
{
    private readonly Dictionary<string, string> _libraryItems = [];
    private LibraryItem(LibraryItemId id) : base(id) { }    
    public LibraryId LibraryId { get; private set; } = LibraryId.Empty;

    public string Path { get; private set; } = string.Empty;
    public IReadOnlyDictionary<string, string> MetaData => _libraryItems;
    
    public static LibraryItem Create(Library library, string path)
    {
        var item = new LibraryItem(LibraryItemId.New());
        item.RecordEvent(new LibraryItemCreated(item.Id, library.Id, path));
        return item;
    }

    public void AddMetaData(string name, string value)
    {
        RecordEvent(new LibraryItemMetaDataAdded(Id, name, value));
    }

    internal void Apply(LibraryItemCreated @event)
    {
        LibraryId = @event.LibraryId;
        Path = @event.Path;
    }

    internal void Apply(LibraryItemMetaDataAdded @event)
    {
        _libraryItems[@event.Name] = @event.Value;
    }

}
