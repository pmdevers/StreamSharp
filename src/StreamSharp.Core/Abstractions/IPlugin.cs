namespace StreamSharp.Core.Abstractions;

public interface IPlugin
{
    public string Name { get; }
    public string Description { get; }

    void Start(IPluginContext context);
    void Stop();
}
