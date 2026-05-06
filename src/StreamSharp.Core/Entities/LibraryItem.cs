using StreamSharp.Core.Abstractions;
using StreamSharp.Core.Events;

namespace StreamSharp.Core.Entities;

[GenerateId]
public class LibraryItem : Aggregate
{
    private readonly Dictionary<string, string> _metadata = [];

    private LibraryItem(AggregateId id) : base(id)
    {
    }

    public AggregateId LibraryId { get; private set; } = AggregateId.Empty;
    public string Path { get; private set; } = string.Empty;
    public IReadOnlyDictionary<string, string> MetaData => _metadata.AsReadOnly();
    
    public static LibraryItem Create(Library library, string path)
    {
        var item = new LibraryItem(AggregateId.New());
        item.RecordEvent(new LibraryItemCreated(item.Id, library.Id, path));
        return item;
    }

    internal void Apply(LibraryItemCreated e)
    {
        LibraryId = e.LibraryId;
        Path = e.Path;
    }
}
