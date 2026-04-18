using StreamSharp.Core.Abstractions;

namespace StreamSharp.Server.Features.Medialibrary.Events;

public record LibraryCreatedEvent(string Name) : StreamEvent;
