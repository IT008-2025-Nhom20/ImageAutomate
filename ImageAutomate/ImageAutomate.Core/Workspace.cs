/**
 * Workspace.cs
 *
 * Serializable container for a complete workspace including graph and view state.
 */

using System.Text.Json;
using ImageAutomate.Core.Serialization;

namespace ImageAutomate.Core;

/// <summary>
/// Represents a complete workspace that can be saved and loaded from disk.
/// </summary>
public class Workspace
{
    private static readonly JsonSerializerOptions _serializerOptions = new()
    {
        IncludeFields = true,
        WriteIndented = true,
    };

    /// <summary>
    /// Gets or sets the workspace name.
    /// </summary>
    public string Name { get; set; } = "Untitled Workspace";

    /// <summary>
    /// Gets or sets the pipeline graph.
    /// </summary>
    public PipelineGraph? Graph { get; set; }

    /// <summary>
    /// Gets or sets the view state.
    /// </summary>
    public ViewState ViewState { get; set; } = new();

    /// <summary>
    /// Gets or sets custom metadata for the workspace.
    /// </summary>
    public Dictionary<string, object?> Metadata { get; set; } = new();

    /// <summary>
    /// Gets or sets whether to include the $schema property in saved files for IntelliSense support.
    /// </summary>
    public bool IncludeSchemaReference { get; set; } = true;

    /// <summary>
    /// Serializes the workspace to JSON string.
    /// </summary>
    public string ToJson()
    {
        var dto = new WorkspaceDto
        {
            Version = "1.0",
            Name = Name,
            Metadata = Metadata
        };

        if (IncludeSchemaReference)
        {
            dto.Schema = "https://raw.githubusercontent.com/IT007-2025-Nhom20/ImageAutomate/main/docs/workspace-schema.json";
        }

        if (Graph != null)
        {
            dto.Graph = Graph.ToDto();
            dto.ViewState = ViewState.ToDto(Graph.Blocks);
        }

        return JsonSerializer.Serialize(dto, _serializerOptions);
    }

    /// <summary>
    /// Deserializes a workspace from JSON string.
    /// </summary>
    public static Workspace FromJson(string json)
    {
        var dto = JsonSerializer.Deserialize<WorkspaceDto>(json, _serializerOptions);
        if (dto == null)
            throw new InvalidOperationException("Failed to deserialize workspace from JSON.");

        var workspace = new Workspace
        {
            Name = dto.Name,
            Metadata = dto.Metadata
        };

        if (dto.Graph != null)
        {
            workspace.Graph = PipelineGraph.FromDto(dto.Graph);

            if (dto.ViewState != null && workspace.Graph != null)
            {
                workspace.ViewState = ViewState.FromDto(dto.ViewState, workspace.Graph.Blocks);
            }
        }

        return workspace;
    }

    /// <summary>
    /// Saves the workspace to a file.
    /// </summary>
    public void SaveToFile(string filePath)
    {
        var json = ToJson();
        File.WriteAllText(filePath, json);
    }

    /// <summary>
    /// Loads a workspace from a file.
    /// </summary>
    public static Workspace LoadFromFile(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"Workspace file not found: {filePath}", filePath);

        var json = File.ReadAllText(filePath);
        return FromJson(json);
    }
}
