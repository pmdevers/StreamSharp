using StreamSharp.Core.Abstractions;
using StreamSharp.Core.Entities;

namespace StreamSharp.Core.Events;

public record LibraryCreated(LibraryId LibraryId, string Name, DateTime CreatedAt) : DomainEvent;
