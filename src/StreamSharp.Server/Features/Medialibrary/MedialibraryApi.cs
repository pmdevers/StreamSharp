using StreamSharp.Server.Features.Medialibrary.Api;

namespace StreamSharp.Server.Features.Medialibrary;

public static class MedialibraryApi
{
    extension(IServiceCollection services)
    {
        public void AddMedialibraryApi()
        {
            services.AddSingleton<MedialibraryManager>();
        }
    }

    extension(WebApplication app)
    {
        public void MapMedialibraryApi()
        {
            var group = app.MapGroup("/medialibrary").WithTags("Media Library");

            group.MapGet("/", Getlibraries.Handle)
                .WithName("GetLibraries")
                .WithDescription("Retrieves a list of media libraries.");

            group.MapPost("/", CreateLibrary.Handle)
                .WithName("CreateLibrary")
                .WithDescription("Creates a new media library.");

            group.MapGet("/{libraryId}", GetLibraryById.Handle)
                .WithName("GetLibraryById")
                .WithDescription("Retrieves a media library by its ID.");

            group.MapPost("/{libraryId}/scan", ScanLibrary.Handle)
                .WithName("ScanLibrary")
                .WithDescription("Scans a media library for new content.");
        }
    }
}
