using StreamSharp.Core.Events;

namespace StreamSharp.PostgresSQL.Projections;

internal class LibraryItemsProjector
{
    public Task OnLibraryItemCreated(LibraryItemCreated e, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
