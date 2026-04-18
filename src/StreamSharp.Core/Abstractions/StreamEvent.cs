namespace StreamSharp.Core.Abstractions;

public abstract record StreamEvent
{
    public DateTimeOffset OccouredOn { get; internal set; }
};
