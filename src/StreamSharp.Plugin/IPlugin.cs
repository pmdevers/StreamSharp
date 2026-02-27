namespace StreamSharp.Plugin;

public interface IPlugin
{
    public string Name { get; }
    public string Description { get; }

    void Start();
    void Stop();
}
