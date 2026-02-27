using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StreamSharp.SourceGenerators
{
    [Generator]
    public class ValueObjectIdGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            // Register attribute source
            context.RegisterPostInitializationOutput(ctx =>
            {
                var attributeSource = TemplateHelper.LoadTemplate("GenerateIdAttribute.cs");
                var rendered = TemplateHelper.RenderTemplate(attributeSource, new()
                {
                    { "namespace", "StreamSharp" }
                });
                ctx.AddSource("GenerateIdAttribute.g.cs", SourceText.From(rendered, Encoding.UTF8));
            });

            // Find all types with [GenerateId] attribute
            var typeDeclarations = context.SyntaxProvider
                .CreateSyntaxProvider(
                    predicate: static (s, _) => IsSyntaxTargetForGeneration(s),
                    transform: static (ctx, _) => GetSemanticTargetForGeneration(ctx))
                .Where(static m => m is not null);

            // Generate code
            context.RegisterSourceOutput(typeDeclarations, static (spc, source) => Execute(source, spc));
        }

        private static bool IsSyntaxTargetForGeneration(SyntaxNode node)
        {
            return node is ClassDeclarationSyntax { AttributeLists.Count: > 0 } or
                   StructDeclarationSyntax { AttributeLists.Count: > 0 };
        }

        private static TypeDeclarationInfo GetSemanticTargetForGeneration(GeneratorSyntaxContext context)
        {
            var typeDeclaration = (TypeDeclarationSyntax)context.Node;

            foreach (var attributeList in typeDeclaration.AttributeLists)
            {
                foreach (var attribute in attributeList.Attributes)
                {
                    var result = TryCreateTypeDeclarationInfo(attribute, typeDeclaration, context.SemanticModel);
                    if (result is not null)
                        return result;
                }
            }

            return null;
        }

        private static TypeDeclarationInfo TryCreateTypeDeclarationInfo(
            AttributeSyntax attribute,
            TypeDeclarationSyntax typeDeclaration,
            SemanticModel semanticModel)
        {
            var symbol = semanticModel.GetSymbolInfo(attribute).Symbol;
            if (symbol is not IMethodSymbol attributeSymbol)
                return null;

            var attributeType = attributeSymbol.ContainingType;
            var attributeName = attributeType.Name;

            // Match by attribute name only (works regardless of namespace)
            if (attributeName != "GenerateIdAttribute")
                return null;

            var typeSymbol = semanticModel.GetDeclaredSymbol(typeDeclaration);
            if (typeSymbol is null)
                return null;

            var underlyingType = GetUnderlyingType(attribute, semanticModel);
            var namespaceName = typeSymbol.ContainingNamespace.IsGlobalNamespace
                ? null
                : typeSymbol.ContainingNamespace.ToDisplayString();

            return new TypeDeclarationInfo(
                typeSymbol.Name,
                namespaceName,
                underlyingType,
                typeDeclaration is StructDeclarationSyntax
            );
        }

        private static string GetUnderlyingType(AttributeSyntax attribute, SemanticModel semanticModel)
        {
            // Check for typeof argument
            if (attribute.ArgumentList?.Arguments.Count > 0)
            {
                var firstArg = attribute.ArgumentList.Arguments[0];
                if (firstArg.Expression is TypeOfExpressionSyntax typeOf)
                {
                    var typeInfo = semanticModel.GetTypeInfo(typeOf.Type);
                    return typeInfo.Type?.ToDisplayString() ?? "System.Guid";
                }
            }

            return "System.Guid"; // Default
        }

        private static void Execute(TypeDeclarationInfo typeInfo, SourceProductionContext context)
        {
            if (typeInfo is null)
                return;

            var source = GenerateIdClass(typeInfo);
            context.AddSource($"{typeInfo.TypeName}Id.g.cs", SourceText.From(source, Encoding.UTF8));
        }

        private static string GenerateIdClass(TypeDeclarationInfo info)
        {
            var template = TemplateHelper.LoadTemplate("ValueObjectId.cs");

            var parameters = new Dictionary<string, object>
            {
                { "namespace", info.Namespace ?? string.Empty },
                { "type_name", $"{info.TypeName}Id" },
                { "underlying_type", info.UnderlyingType },
                { "keyword", info.IsStruct ? "struct" : "readonly struct" },
                { "is_guid", info.UnderlyingType == "System.Guid" }
            };

            return TemplateHelper.RenderTemplate(template, parameters);
        }

        private sealed class TypeDeclarationInfo(string typeName, string @namespace, string underlyingType, bool isStruct)
        {
            public string TypeName { get; } = typeName;
            public string Namespace { get; } = @namespace;
            public string UnderlyingType { get; } = underlyingType;
            public bool IsStruct { get; } = isStruct;
        }
    }
}
