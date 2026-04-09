using StreamSharp.Server.Features.Medialibrary.Events;
using StreamSharp.Server.Modeling;

namespace StreamSharp.Server.Features.Medialibrary;

[GenerateId]
public class Library(LibraryId id) : AggregateRoot<LibraryId>(id)
{
    public string Name { get; set; }

    public string Path { get; set; }

    internal void Apply(LibraryCreated e)
    {
        Name = e.Name;
    }
}
