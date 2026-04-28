namespace StreamSharp.Core.Entities;

[GenerateId]
public class LibraryItem
{
    public LibraryItemId Id { get; init; }
    public LibraryId LibraryId { get; init; } = LibraryId.Empty;
    public string Path { get; init; } = string.Empty;
    public DateTimeOffset CreatedAt { get; init; }
    public IReadOnlyDictionary<string, string> MetaData { get; set; } = new Dictionary<string, string>();
    
    public static LibraryItem Create(Library library, string path, TimeProvider? timeProvider = null)
    {
        var provider = timeProvider ?? TimeProvider.System;
        var item = new LibraryItem ()
        {
            Id = LibraryItemId.New(),
            LibraryId = library.Id,
            Path = path,
            CreatedAt = provider.GetUtcNow()
        };
        
        return item;
    }
}
