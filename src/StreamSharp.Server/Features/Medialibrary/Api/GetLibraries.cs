namespace StreamSharp.Server.Features.Medialibrary;

public static class Getlibraries
{
    public static async Task<IResult> Handle()
    {

        return TypedResults.Ok(new[] { new { Id = "1", Name = "My Media Library" } });
    }
}
