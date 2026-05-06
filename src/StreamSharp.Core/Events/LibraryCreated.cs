using StreamSharp.Core.Abstractions;

namespace StreamSharp.Core.Events;

public record LibraryCreated(AggregateId LibraryId, string Name) : DomainEvent;
