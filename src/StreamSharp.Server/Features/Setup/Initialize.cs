namespace StreamSharp.Server.Features.Setup;

public static class Initialize
{
    public record InitializeRequest(string MediaDirectory, string DatabasePath);
    public static async Task<IResult> Handle(InitializeRequest request)
    {
        // Here you would add logic to initialize your application with the provided configuration.
        // For example, you might set up the media directory and database path in your application's settings.
        // Simulate some initialization work
        await Task.Delay(1000);
        return Results.Ok("Application initialized successfully.");
    }
}
