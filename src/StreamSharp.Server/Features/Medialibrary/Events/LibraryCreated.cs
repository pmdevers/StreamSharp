using StreamSharp.Server.Modeling;

namespace StreamSharp.Server.Features.Medialibrary.Events;

public record LibraryCreated(string Name) : EventRecord;
