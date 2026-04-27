using Microsoft.AspNetCore.Mvc;
using StreamSharp.Core.Entities;
using StreamSharp.Core.Storage;

namespace StreamSharp.Server.Features.Medialibrary;

public static class GetLibraryById
{
    public static async Task<IResult> Handle(
        [FromServices] LibraryRepository store,
        LibraryId libraryId)
    {
        var library = await store.GetAsync(libraryId);

        return TypedResults.Ok(new { LibraryId = library.Id, library.Name });
    }
}
