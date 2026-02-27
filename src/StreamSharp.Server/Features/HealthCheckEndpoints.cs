namespace StreamSharp.Server.Features;

public static class HealthCheckEndpoints
{
    extension(WebApplication app)
    {
        public void HealthChecks()
        {
            app.MapGet("/healthz", () => Results.Ok("Healthy")).WithTags("Media Library"); ;
            app.MapGet("/readyz", () => Results.Ok("Ready")).WithTags("Media Library");
            app.MapGet("/livez", () => Results.Ok("Alive")).WithTags("Media Library");
        }
    }
}
