using StreamSharp.Core.Entities;

namespace StreamSharp.Server.Features.Medialibrary;

public static class ScanLibrary
{
    public static async Task<IResult> Handle(LibraryId libraryId)
    {
        return TypedResults.Ok(new { LibraryId = libraryId, Name = "My Media Library" });
    }
}
