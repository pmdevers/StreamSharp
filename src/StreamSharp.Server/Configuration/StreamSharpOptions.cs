using System.Runtime.CompilerServices;

namespace StreamSharp.Server.Configuration;

public class StreamSharpOptions : Dictionary<string, object>
{
    public StreamSharpOptions() { }
    public StreamSharpOptions(IDictionary<string, object> options)
        : base(options)
    { }
    public string Name
    {
        get => GetValue<string>();
        set => SetValue(value);
    }

    public string PluginsRoot
    {
        get => GetValue("plugins");
        set => SetValue(value);
    }

    public void SetValue<T>(T value, [CallerMemberName] string key = "")
    {
        this[key] = value!;
    }

    public T GetValue<T>(T defaultValue = default!, [CallerMemberName] string key = "")
    {
        if (TryGetValue(key, out var value) && value is T typedValue)
        {
            return typedValue;
        }
        return defaultValue;
    }
}
