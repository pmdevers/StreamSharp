namespace StreamSharp.Server.Features;

public static class HealthCheckEndpoints
{
    extension(WebApplication app)
    {
        public void HealthChecks()
        {
            var group = app.MapGroup("/healthz").WithTags("Health Checks");

            group.MapGet("/", () => Results.Ok("Healthy"));
            group.MapGet("/readyz", () => Results.Ok("Ready"));
            group.MapGet("/livez", () => Results.Ok("Alive"));
        }
    }
}
