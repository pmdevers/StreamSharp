using Microsoft.AspNetCore.Mvc;
using StreamSharp.Server.Configuration;

namespace StreamSharp.Server.Features;

public static class RedirectToBasePathFeature
{
    extension(WebApplication app)
    {
        public void RedirectToBasePath()
        {
            var options = app.Services.GetRequiredService<StreamSharpOptions>();
            if (options.BasePath == "/")
                return;

            app.UsePathBase(options.BasePath);
            app.Use(next =>
            {
                return async context =>
                {
                    if (!context.Request.PathBase.StartsWithSegments(options.BasePath))
                    {
                        context.Response.Redirect(options.BasePath + context.Request.Path);
                        return;
                    }

                    await next(context);
                };
            });


            app.MapGet("/restart", async ([FromServices] ServerHost server) => await server.StopAsync());
            app.MapGet("/shutdown", async ([FromServices] ServerHost server) => await server.ServerStopAsync());
        }
    }
}
