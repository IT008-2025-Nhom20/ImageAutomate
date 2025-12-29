/**
 * BlockSerializer.cs
 *
 * Handles serialization and deserialization of IBlock implementations.
 */

using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace ImageAutomate.Core.Serialization;

/// <summary>
/// Provides serialization/deserialization for IBlock instances.
/// </summary>
public static class BlockSerializer
{
    private static readonly JsonSerializerOptions _serializerOptions = new JsonSerializerOptions
    {
        IncludeFields = true,
        WriteIndented = false
    };

    /// <summary>
    /// Serializes an IBlock to a BlockDto.
    /// Layout properties (X, Y, Width, Height) are serialized as regular properties.
    /// </summary>
    public static BlockDto Serialize(IBlock block)
    {
        ArgumentNullException.ThrowIfNull(block);

        var properties = block.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
        Dictionary<string, object?> propertyDict = [];
        foreach (var prop in properties)
        {
            // Skip properties that are part of IBlock interface (except layout properties)
            if (prop.Name == nameof(IBlock.Name) ||
                prop.Name == nameof(IBlock.Title) ||
                prop.Name == nameof(IBlock.Content) ||
                prop.Name == nameof(IBlock.Inputs) ||
                prop.Name == nameof(IBlock.Outputs))
                continue;

            // Layout properties are always serialized
            bool isLayoutProperty = prop.Name is nameof(IBlock.X) or nameof(IBlock.Y)
                or nameof(IBlock.Width) or nameof(IBlock.Height);

            // Skip non-layout properties without Category attribute
            if (!isLayoutProperty)
            {
                var categoryAttr = prop.GetCustomAttribute<CategoryAttribute>();
                if (categoryAttr == null)
                    continue;
            }

            // Skip read-only properties
            if (!prop.CanWrite)
                continue;

            try
            {
                var value = prop.GetValue(block);
                propertyDict[prop.Name] = SerializePropertyValue(value, prop.PropertyType);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to serialize property {prop.Name}: {ex.Message}");
            }
        }

        // Serialize properties (including layout: X, Y, Width, Height)
        return new BlockDto(
            block.GetType().Name,
            block.GetType().AssemblyQualifiedName ?? block.GetType().FullName ?? block.GetType().Name,
            propertyDict,
            block.Inputs.Select(s => new SocketDto(s)).ToList(),
            block.Outputs.Select(s => new SocketDto(s)).ToList()
        );
    }

    /// <summary>
    /// Deserializes a BlockDto to an IBlock instance.
    /// </summary>
    public static IBlock Deserialize(BlockDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);
        // Get the type from the assembly qualified name
        var type = Type.GetType(dto.AssemblyQualifiedName);
        if (type == null)
            throw new InvalidOperationException($"Cannot find type: {dto.AssemblyQualifiedName}");

        // Create instance
        var block = Activator.CreateInstance(type) as IBlock;
        if (block == null)
            throw new InvalidOperationException($"Cannot create instance of type: {type.FullName}");

        // Restore properties (including layout)
        foreach (var kvp in dto.Properties)
        {
            var prop = type.GetProperty(kvp.Key, BindingFlags.Public | BindingFlags.Instance);
            if (prop != null && prop.CanWrite)
            {
                try
                {
                    var value = DeserializePropertyValue(kvp.Value, prop.PropertyType);
                    prop.SetValue(block, value);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Failed to deserialize property {kvp.Key}: {ex.Message}");
                }
            }
        }

        return block;
    }

    private static object? SerializePropertyValue(object? value, Type propertyType)
    {
        if (value == null)
            return null;

        // Handle enums
        if (propertyType.IsEnum)
            return value.ToString();

        // Handle primitive types and strings
        if (propertyType.IsPrimitive || propertyType == typeof(string) || propertyType == typeof(decimal))
            return value;

        // Handle nullable types
        if (Nullable.GetUnderlyingType(propertyType) != null)
        {
            var underlyingType = Nullable.GetUnderlyingType(propertyType)!;
            if (underlyingType.IsEnum)
                return value.ToString();
            if (underlyingType.IsPrimitive || underlyingType == typeof(string) || underlyingType == typeof(decimal))
                return value;
        }

        // For complex objects, serialize to JSON
        var json = JsonSerializer.Serialize(value, _serializerOptions);
        return JsonNode.Parse(json);
    }

    private static object? DeserializePropertyValue(object? value, Type targetType)
    {
        if (value == null)
            return null;

        // Handle enums
        if (targetType.IsEnum)
        {
            if (value is string strValue)
                return Enum.Parse(targetType, strValue);
            if (value is JsonElement jsonElement && jsonElement.ValueKind == JsonValueKind.String)
                return Enum.Parse(targetType, jsonElement.GetString()!);
        }

        // Handle nullable enums
        var underlyingType = Nullable.GetUnderlyingType(targetType);
        if (underlyingType != null && underlyingType.IsEnum)
        {
            if (value is string strValue)
                return Enum.Parse(underlyingType, strValue);
            if (value is JsonElement jsonElement && jsonElement.ValueKind == JsonValueKind.String)
                return Enum.Parse(underlyingType, jsonElement.GetString()!);
        }

        // Handle JsonElement
        if (value is JsonElement element)
        {
            return JsonSerializer.Deserialize(element.GetRawText(), targetType);
        }

        // Handle JsonNode
        if (value is JsonNode node)
        {
            return JsonSerializer.Deserialize(node.ToJsonString(), targetType);
        }

        // Try direct conversion for primitive types
        if (targetType.IsPrimitive || targetType == typeof(string) || targetType == typeof(decimal))
        {
            return Convert.ChangeType(value, targetType, CultureInfo.InvariantCulture);
        }

        // Handle nullable primitives
        if (underlyingType != null && (underlyingType.IsPrimitive || underlyingType == typeof(string) || underlyingType == typeof(decimal)))
        {
            return Convert.ChangeType(value, underlyingType, CultureInfo.InvariantCulture);
        }

        // For complex types, try JSON deserialization
        var json = JsonSerializer.Serialize(value);
        return JsonSerializer.Deserialize(json, targetType);
    }
}