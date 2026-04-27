using StreamSharp.Core.Abstractions;
using StreamSharp.Core.Entities;

namespace StreamSharp.Core.Events;

public record LibraryItemMetaDataAdded(LibraryItemId LibraryItemId, string Name, string Value) : DomainEvent;
