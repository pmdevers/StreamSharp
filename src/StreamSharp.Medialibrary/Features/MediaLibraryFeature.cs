using Microsoft.AspNetCore.Mvc;
using StreamSharp.Server.Configuration;

namespace StreamSharp.Medialibrary.Features;

public static class MedialibraryFeature
{
    extension<T>(T route)
        where T : IEndpointRouteBuilder
    {
        public void MapMedialibrary()
        {

        }
    }

    public static async Task<IResult> Getlibraries(
        [FromServices] StreamSharpOptions options
        )
    {

        return TypedResults.Ok(new[] { new { Id = "1", Name = "My Media Library" } });
    }

    public record LibraryRequest();
    public static async Task<IResult> CreateLibrary(LibraryRequest request)
    {
        return TypedResults.Created($"/library/{Guid.NewGuid()}", request);
    }

    public static async Task<IResult> GetLibraryById(string libraryId)
    {
        return TypedResults.Ok(new { Id = libraryId, Name = "My Media Library" });
    }

    public static async Task<IResult> ScanLibrary(string libraryId)
    {
        return TypedResults.Ok(new { Id = libraryId, Name = "My Media Library", Status = "Scan started" });
    }
}
