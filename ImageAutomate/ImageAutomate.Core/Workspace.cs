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

    private PipelineGraph? _graph;

    /// <summary>
    /// Gets or sets the workspace name.
    /// </summary>
    public string Name { get; set; } = "Untitled Workspace";

    /// <summary>
    /// Gets or sets the pipeline graph.
    /// </summary>
    public PipelineGraph? Graph
    {
        get => _graph;
        set
        {
            // Unsubscribe from old graph
            if (_graph != null)
            {
                _graph.OnNodeRemoved -= OnGraphNodeRemoved;
            }

            _graph = value;

            // Subscribe to new graph
            if (_graph != null)
            {
                _graph.OnNodeRemoved += OnGraphNodeRemoved;
            }
        }
    }

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
    /// Handles automatic cleanup of ViewState when a block is removed from the graph.
    /// </summary>
    private void OnGraphNodeRemoved(IBlock block)
    {
        ViewState.RemoveBlock(block);
    }

    /// <summary>
    /// Finds the top-most block at the given world coordinates.
    /// </summary>
    public IBlock? HitTestNode(double x, double y)
    {
        if (Graph == null)
            return null;

        // Iterate in reverse order (Top to Bottom)
        for (int i = Graph.Nodes.Count - 1; i >= 0; i--)
        {
            var block = Graph.Nodes[i];
            var blockPos = ViewState.GetBlockPositionOrDefault(block);
            var blockSize = ViewState.GetBlockSizeOrDefault(block);

            if (x >= blockPos.X && x <= blockPos.X + blockSize.Width &&
                y >= blockPos.Y && y <= blockPos.Y + blockSize.Height)
            {
                return block;
            }
        }
        return null;
    }

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
            dto.Schema = "https://raw.githubusercontent.com/IT007-2025-Nhom20/ImageAutomate/project-restructure/docs/workspace-schema.json";
        }

        if (Graph != null)
        {
            // Pass ViewState to embed layout in each block
            dto.Graph = Graph.ToDto(ViewState);
            dto.ViewState = ViewState.ToDto();
        }

        return JsonSerializer.Serialize(dto, _serializerOptions);
    }

    /// <summary>
    /// Deserializes a workspace from JSON string.
    /// </summary>
    public static Workspace FromJson(string json)
    {
        var dto = JsonSerializer.Deserialize<WorkspaceDto>(json, _serializerOptions)
            ?? throw new InvalidOperationException("Failed to deserialize workspace from JSON.");
        
        var workspace = new Workspace
        {
            Name = dto.Name,
            Metadata = dto.Metadata
        };

        if (dto.Graph != null)
        {
            // Create ViewState first, then pass to FromDto to extract layout from blocks
            if (dto.ViewState != null)
            {
                workspace.ViewState = ViewState.FromDto(dto.ViewState);
            }
            
            // Pass ViewState to extract embedded layout from blocks
            workspace.Graph = PipelineGraph.FromDto(dto.Graph, workspace.ViewState);
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
