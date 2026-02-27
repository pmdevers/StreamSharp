using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace StreamSharp.SourceGenerators
{
    internal static class TemplateHelper
    {
        private static readonly Assembly ThisAssembly = typeof(TemplateHelper).Assembly;
        private static readonly Dictionary<string, string> TemplateCache = [];

        public static string LoadTemplate(string templateName)
        {
            if (TemplateCache.TryGetValue(templateName, out var cached))
            {
                return cached;
            }

            // Support both .cs and bare names
            var resourceName = templateName.EndsWith(".cs")
                ? $"StreamSharp.SourceGenerators.Templates.{templateName}"
                : $"StreamSharp.SourceGenerators.Templates.{templateName}.cs";

            using var stream = ThisAssembly.GetManifestResourceStream(resourceName)
                ?? throw new InvalidOperationException($"Could not find embedded resource: {resourceName}");

            using var reader = new StreamReader(stream, Encoding.UTF8);
            var content = reader.ReadToEnd();
            TemplateCache[templateName] = content;
            return content;
        }

        public static string RenderTemplate(string template, Dictionary<string, object> parameters)
        {
            var result = template;

            foreach (var param in parameters)
            {
                var placeholder = $"{{{{ {param.Key} }}}}";
                var value = param.Value?.ToString() ?? string.Empty;
                result = result.Replace(placeholder, value);

                // Handle conditional blocks: {{~ if param_name ~}} ... {{~ end ~}}
                var ifBlock = $"{{{{~ if {param.Key} ~}}}}";
                var endBlock = "{{~ end ~}}";

                while (result.Contains(ifBlock))
                {
                    var startIndex = result.IndexOf(ifBlock);
                    var endIndex = result.IndexOf(endBlock, startIndex);

                    if (endIndex > startIndex)
                    {
                        var blockContent = result.Substring(
                            startIndex + ifBlock.Length,
                            endIndex - (startIndex + ifBlock.Length));

                        // Check if condition is true
                        var isTrue = param.Value is bool b ? b : !string.IsNullOrEmpty(value);
                        var replacement = isTrue ? blockContent : string.Empty;

                        result = result.Remove(startIndex, endIndex + endBlock.Length - startIndex);
                        result = result.Insert(startIndex, replacement);
                    }
                    else
                    {
                        break;
                    }
                }
            }

            return result;
        }
    }
}
