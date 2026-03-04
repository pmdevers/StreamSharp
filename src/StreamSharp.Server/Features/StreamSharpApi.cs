using StreamSharp.Server.Features.Plugins;
using StreamSharp.Server.Features.Server;

namespace StreamSharp.Server.Features;

public static class StreamSharpApi
{
    extension(WebApplication app)
    {
        public void MapStreamSharpApi()
        {
            app.MapPluginsApi();
            app.MapServerApi();
        }
    }
}
