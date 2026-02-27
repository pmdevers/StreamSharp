namespace StreamSharp.Server.Features.Library;

public static class CreateLibrary
{
    public record CreateLibraryRequest(string Name);

    public static async Task<IResult> Handle(CreateLibraryRequest request)
    {
        // Here you would typically call a service to create the library and persist it
        var newLibraryId = Guid.NewGuid(); // Simulate creating a new library ID
        // Return a Created response with the location of the new library
        return TypedResults.Created($"/library/{newLibraryId}", new { Id = newLibraryId, Name = request.Name });
    }
}
