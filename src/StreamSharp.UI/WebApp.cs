using Microsoft.Extensions.FileProviders;
using System.Reflection;

namespace StreamSharp.UI;

public static class WebApp
{
    public static IFileProvider CreateFileProvider()
    {
        var assembly = Assembly.GetExecutingAssembly();
        return new ManifestEmbeddedFileProvider(assembly, "dist");
    }
}
