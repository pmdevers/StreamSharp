using StreamSharp.Core.Entities;
using StreamSharp.Core.Events;

namespace StreamSharp.PostgreSQL.Projections;

internal class LibraryProjector(StreamSharpDB context)
{
    public async Task OnLibraryCreated(LibraryCreated e, CancellationToken cancellationToken)
    {
        context.Libraries.Add(new Library()
        {
            Id = e.LibraryId,
            Name = e.Name,
            CreatedAt = e.CreatedAt
        });

        await context.SaveChangesAsync(cancellationToken);
    }
}
