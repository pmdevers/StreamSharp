namespace StreamSharp.Core.Entities;

[GenerateId]
public class Library()
{
    public LibraryId Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public DateTimeOffset CreatedAt { get; init; }

    public static Library Create(string name, TimeProvider? timeProvider = null)
    {
        var provider = (timeProvider ?? TimeProvider.System);
        var library = new Library
        {
            Id = LibraryId.New(),
            Name = name,
            CreatedAt = provider.GetUtcNow()
        };
        return library;
    }

    public LibraryItem CreateItem(string path, TimeProvider? timeProvider = null)
    {
        return LibraryItem.Create(this, path, timeProvider);
    }
}
