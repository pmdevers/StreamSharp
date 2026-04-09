using Microsoft.AspNetCore.Mvc;
using StreamSharp.Server.Modeling;

namespace StreamSharp.Server.Features.Medialibrary.Api;

public static class GetLibraryById
{
    public static async Task<IResult> Handle(
        [FromServices] IEventStreamProvider<LibraryId> manager,
        LibraryId libraryId)
    {
        var stream = await manager.GetOrCreateStream(libraryId);

        if (stream is null)
            return TypedResults.NotFound(libraryId);

        var library = AggregateRoot.LoadFromHistory<Library, LibraryId>(stream);

        return TypedResults.Ok(new { library.Id, library.Name });
    }
}
