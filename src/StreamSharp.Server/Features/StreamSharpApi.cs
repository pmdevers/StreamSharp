using StreamSharp.Server.Features.Medialibrary;
using StreamSharp.Server.Features.Plugins;
using StreamSharp.Server.Features.Server;

namespace StreamSharp.Server.Features;

public static class StreamSharpApi
{
    extension(IEndpointRouteBuilder app)
    {
        public void MapStreamSharpApi()
        {
            var group = app.MapGroup("/api").WithTags("StreamSharp API");

            group.MapMedialibraryApi();
            group.MapPluginsApi();
            group.MapServerApi();
        }
    }
}
