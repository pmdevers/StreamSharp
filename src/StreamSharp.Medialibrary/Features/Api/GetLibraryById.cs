using Microsoft.AspNetCore.Mvc;

namespace StreamSharp.Server.Features.Medialibrary.Api;

public static class GetLibraryById
{
    public static async Task<IResult> Handle(
        [FromServices] MedialibraryManager manager,
        LibraryId libraryId)
    {
        var library = await manager.FindById(libraryId);

        if (library is null)
            return TypedResults.NotFound(libraryId);

        return TypedResults.Ok(new { library.Id, library.Name });
    }
}
