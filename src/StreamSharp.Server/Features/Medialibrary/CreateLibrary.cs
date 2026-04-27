using Microsoft.AspNetCore.Mvc;
using StreamSharp.Core.Entities;
using StreamSharp.Core.Storage;

namespace StreamSharp.Server.Features.Medialibrary;

public static class CreateLibrary
{
    public record CreateLibraryRequest(string Name);

    public static async Task<IResult> Handle(
        [FromServices] LibraryRepository libraryRepository,
        [FromServices] LibraryItemRepository libraryItemRepository,
        [FromBody] CreateLibraryRequest request)
    {
        var library = Library.Create(request.Name);
    
        var libraryItem = library.CreateItem("/path/to/media/file");

        await libraryRepository.SaveAsync(library);
        await libraryItemRepository.SaveAsync(libraryItem);

        return Results.Ok(library);
    }
}
