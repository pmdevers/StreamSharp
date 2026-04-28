using StreamSharp.Core.Abstractions;
using System.Text.Json;

namespace StreamSharp.PostgreSQL;

/// <summary>
/// Handles serialization and deserialization of domain events
/// </summary>
public static class EventSerializer
{
    /// <summary>
    /// Serialize a domain event to JSON
    /// </summary>
    public static string Serialize(DomainEvent domainEvent)
    {
        return JsonSerializer.Serialize(domainEvent, domainEvent.GetType());
    }

    /// <summary>
    /// Deserialize a domain event from JSON using the specified type name
    /// </summary>
    public static DomainEvent? Deserialize(string json, string typeName)
    {
        var type = ResolveType(typeName);
        if (type == null)
            return null;

        return JsonSerializer.Deserialize(json, type) as DomainEvent;
    }

    /// <summary>
    /// Get the assembly-qualified name of an event type
    /// </summary>
    public static string GetTypeName(DomainEvent domainEvent)
    {
        return domainEvent.GetType().AssemblyQualifiedName ?? domainEvent.GetType().FullName ?? string.Empty;
    }

    /// <summary>
    /// Resolve a type name to a Type object by searching loaded assemblies
    /// </summary>
    private static Type? ResolveType(string typeName)
    {
        // Try standard resolution first
        var type = Type.GetType(typeName);
        if (type != null)
            return type;

        // Search through all loaded assemblies
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            type = assembly.GetType(typeName);
            if (type != null)
                return type;
        }

        return null;
    }
}
