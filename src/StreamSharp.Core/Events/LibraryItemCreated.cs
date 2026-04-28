using StreamSharp.Core.Abstractions;
using StreamSharp.Core.Entities;

namespace StreamSharp.Core.Events;

public record LibraryItemCreated(LibraryItemId LibraryItemId, LibraryId LibraryId, string Path) : DomainEvent;
