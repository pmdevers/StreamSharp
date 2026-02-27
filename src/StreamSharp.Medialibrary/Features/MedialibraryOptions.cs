using StreamSharp.Server.Configuration;

namespace StreamSharp.Medialibrary.Features;

public static class MedialibraryOptions
{
    extension(StreamSharpOptions options)
    {
        public string LibraryPath
        {
            get => options.GetValue(Environment.GetEnvironmentVariable("LIBRARY_PATH") ?? string.Empty);
            set => options.SetValue(value);
        }
    }
}
