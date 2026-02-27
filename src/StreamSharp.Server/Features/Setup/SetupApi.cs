namespace StreamSharp.Server.Features.Setup;

public static class SetupApi
{
    extension(WebApplication app)
    {
        public void MapSetupApi()
        {
            var group = app.MapGroup("/setup").WithTags("Setup");

            group.MapPost("/initialize", Initialize.Handle)
                .WithName("Initialize")
                .WithDescription("Initializes the application with the provided configuration.");

            group.MapGet("/finish", Finish.Handle).WithName("SetupFinished")
                .WithDescription("Checks if the setup process has been completed.");
        }
    }
}
