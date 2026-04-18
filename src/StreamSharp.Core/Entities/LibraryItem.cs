namespace StreamSharp.Core.Entities;

[GenerateId]
public class LibraryItem
{
    private LibraryItem() { }
    public LibraryItemId Id { get; init; } = LibraryItemId.New();
    public LibraryId LibraryId { get; init; }

    public required string Name { get; init; }
    public required string Path { get; init; }

    public static LibraryItem Create(Library library, string name, string path)
        => new()
        {
            LibraryId = library.Id,
            Name = name,
            Path = path
        };

    public LibraryItemMetaData CreateMetaData(string name, string value)
        => LibraryItemMetaData.Create(this, name, value);

}

[GenerateId]
public class LibraryItemMetaData
{
    private LibraryItemMetaData() { }
    public LibraryItemMetaDataId Id { get; init; } = LibraryItemMetaDataId.New();
    public LibraryItemId LibraryItemId { get; private set; } = LibraryItemId.Empty;
    public string Name { get; private set; } = string.Empty;
    public string Value { get; private set; } = string.Empty;

    public static LibraryItemMetaData Create(LibraryItem item, string name, string value)
        => new()
        {
            LibraryItemId = item.Id,
            Name = name,
            Value = value
        };
}
