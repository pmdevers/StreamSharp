using StreamSharp.Plugin;
using StreamSharp.Server.Features.Medialibrary;
using StreamSharp.Server.Features.Medialibrary.Api;

namespace StreamSharp.Medialibrary;

public class Medialibrary : IPlugin
{
    public string Name => "Media Library";

    public string Description =>
        "Provides media management capabilities, including metadata handling, media organization, and integration with external media sources.";

    public void Start(IPluginContext context)
    {
        context.RegisterServices(services =>
        {
            services.AddSingleton<MedialibraryManager>();
        });

        context.RegisterApi(endpoints =>
        {
            endpoints.MapGet("/", Getlibraries.Handle)
                 .WithName("GetLibraries")
                 .WithDescription("Retrieves a list of media libraries.");

            endpoints.MapPost("/", CreateLibrary.Handle)
                .WithName("CreateLibrary")
                .WithDescription("Creates a new media library.");

            endpoints.MapGet("/{libraryId}", GetLibraryById.Handle)
                .WithName("GetLibraryById")
                .WithDescription("Retrieves a media library by its ID.");

            endpoints.MapPost("/{libraryId}/scan", ScanLibrary.Handle)
                .WithName("ScanLibrary")
                .WithDescription("Scans a media library for new content.");
        });
    }

    public void Stop()
    {

    }
}
