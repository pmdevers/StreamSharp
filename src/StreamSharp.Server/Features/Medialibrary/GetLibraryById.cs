using Microsoft.AspNetCore.Mvc;
using StreamSharp.Core.Entities;

namespace StreamSharp.Server.Features.Medialibrary;

public static class GetLibraryById
{
    public static async Task<IResult> Handle(
        [FromServices] ILibraryStore store,
        LibraryId libraryId)
    {
        var library = await store.LoadAsync(libraryId);

        return TypedResults.Ok(new { LibraryId = library.Id, library.Name });
    }
}
