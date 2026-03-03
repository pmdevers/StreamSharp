namespace StreamSharp.Server.Features.Medialibrary;

public static class ScanLibrary
{
    public static async Task<IResult> Handle(string libraryId)
    {
        return TypedResults.Ok(new { Id = libraryId, Name = "My Media Library" });
    }
}
