using Microsoft.AspNetCore.Mvc;

namespace StreamSharp.Server.Features.Medialibrary.Api;

public static class CreateLibrary
{
    public record CreateLibraryRequest(string Name);

    public static async Task<IResult> Handle(
        [FromServices] MedialibraryManager manager,
        CreateLibraryRequest request)
    {
        var library = await manager.CreateAsync(new Library()
        {
            Id = LibraryId.New(),
            Name = request.Name
        });

        return TypedResults.Created($"/library/{library.Id}", new { library.Id, library.Name });
    }
}
