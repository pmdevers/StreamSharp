using StreamSharp.Server.Configuration;

namespace StreamSharp.Server;

public static class ServerHostOptions
{
    extension(StreamSharpOptions options)
    {
        public bool EnableRestartOnFailure
        {
            get => options.GetValue(true);
            set => options.SetValue(value);
        }

        public string BasePath
        {
            get => options.GetValue("/");
            set => options.SetValue(NormalizeBasePath(value));
        }

        public string PluginsRoot
        {
            get => options.GetValue(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "plugins"));
            set => options.SetValue(value);
        }

        public bool LoadPlugins
        {
            get => options.GetValue(true);
            set => options.SetValue(value);
        }

        private static string NormalizeBasePath(string? path)
        {
            if (string.IsNullOrWhiteSpace(path) || path == "/")
                return "/";

            path = path.Trim();

            if (!path.StartsWith('/'))
                path = $"/{path}";

            if (path.EndsWith('/'))
                path = path.TrimEnd('/');
            return path;
        }
    }
}
