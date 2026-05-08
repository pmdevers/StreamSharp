using StreamSharp.Core.Abstractions;
using StreamSharp.Core.Entities;
using StreamSharp.Core.Events;
using static StreamSharp.Core.Queries.ILibraryQueries;

namespace StreamSharp.PostgreSQL.Projections;

internal class LibraryProjector(StreamSharpDB context)
{
    public async Task OnLibraryCreated(LibraryCreated e, CancellationToken cancellationToken)
    {
        context.Libraries.Add(new LibraryDto { Id = e.LibraryId, Name = e.Name, CreatedAt = e.OccurredOn });
        await context.SaveChangesAsync(cancellationToken);
    }
}
