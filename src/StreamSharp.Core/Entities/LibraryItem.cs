using StreamSharp.Core.Abstractions;
using StreamSharp.Core.Events;

namespace StreamSharp.Core.Entities;

[GenerateId]
public class LibraryItem : AggregateRoot<LibraryItemId>
{
    private readonly Dictionary<string, string> _metadata = [];

    private LibraryItem(LibraryItemId id) : base(id)
    {
    }

    public LibraryId LibraryId { get; private set; } = LibraryId.Empty;
    public string Path { get; private set; } = string.Empty;
    public IReadOnlyDictionary<string, string> MetaData => _metadata.AsReadOnly();
    
    public static LibraryItem Create(Library library, string path)
    {
        var item = new LibraryItem(LibraryItemId.New());
        item.RecordEvent(new LibraryItemCreated(item.Id, library.Id, path));
        return item;
    }

    internal void Apply(LibraryItemCreated e)
    {
        LibraryId = e.LibraryId;
        Path = e.Path;
    }
}
