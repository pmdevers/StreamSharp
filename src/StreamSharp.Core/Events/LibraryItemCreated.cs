using StreamSharp.Core.Abstractions;

namespace StreamSharp.Core.Events;

public record LibraryItemCreated(AggregateId LibraryItemId, AggregateId LibraryId, string Path) : DomainEvent;
