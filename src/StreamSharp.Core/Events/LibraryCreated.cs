using StreamSharp.Core.Abstractions;

namespace StreamSharp.Server.Features.Medialibrary.Events;

public record LibraryCreated(string Name) : DomainEvent;
