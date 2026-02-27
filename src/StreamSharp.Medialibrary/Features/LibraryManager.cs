namespace StreamSharp.Medialibrary.Features;

public class LibraryManager
{
    public Library GetLibrary(Guid libraryId)
    {
        // Implementation to retrieve a library by its ID
        return new Library { Id = LibraryId.From(libraryId), Name = "My Media Library" };
    }
}

[GenerateId]
public class Library
{
    public LibraryId Id { get; init; }
    public string Name { get; init; }

}
public record LibraryInfo(string Id, string Name);
