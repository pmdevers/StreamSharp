namespace StreamSharp.Server.Features.Medialibrary;

[GenerateId]
public class LibraryItem
{
    public LibraryItemId Id { get; set; }
    public LibraryId LibraryId { get; set; }
    public string Name { get; set; }

}
