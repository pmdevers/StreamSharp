using Microsoft.AspNetCore.Mvc;
using StreamSharp.Core.Abstractions;
using StreamSharp.Core.Entities;

namespace StreamSharp.Server.Features.Medialibrary;

public static class ScanLibrary
{
    public static async Task<IResult> Handle(
        [FromServices] IUnitOfWork unitOfWork,
        AggregateId libraryId,
        CancellationToken cancellationToken)
    {

        var repository = unitOfWork.GetRepository<Library>();
        var repositoryItem = unitOfWork.GetRepository<LibraryItem>();
        
        var libary = await repository.TryFindAsync(libraryId, cancellationToken);

        if (libary == null)
        {
            return TypedResults.NotFound();
        }

        Directory.EnumerateFileSystemEntries("z:\\movies").ToList().ForEach(path =>
        {
            var item = libary.CreateItem(path);
            repositoryItem.Add(item);
        });

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return TypedResults.Ok(new { LibraryId = libraryId, Name = "My Media Library" });
    }
}
